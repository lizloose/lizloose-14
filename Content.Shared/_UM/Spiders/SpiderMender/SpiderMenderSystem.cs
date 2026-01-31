using Content.Shared._UM.Energy;
using Content.Shared.Actions;
using Content.Shared.Body.Systems;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Whitelist;

namespace Content.Shared._UM.Spiders.SpiderMender;

/// <summary>
/// This handles...
/// </summary>
public sealed class SpiderMenderSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly EnergyContainerSystem _energy = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstreamSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpiderMenderComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SpiderMenderComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SpiderMenderComponent, OnSpiderMendActionEvent>(OnMendAction);
        SubscribeLocalEvent<SpiderMenderComponent, SpiderMendDoAfterEvent>(OnSpiderMendDoAfter);

    }

    private void OnMapInit(Entity<SpiderMenderComponent> ent, ref MapInitEvent args)
    {
        _actionsSystem.AddAction(ent, ref ent.Comp.MendActionEntity, ent.Comp.MendAction);
    }

    private void OnShutdown(Entity<SpiderMenderComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.MendActionEntity != null)
        {
            _actionsSystem.RemoveAction(ent.Owner, ent.Comp.MendActionEntity);
        }
    }

    private bool HasDamage(Entity<SpiderMenderComponent> healing, Entity<DamageableComponent?> target)
    {
        if (!Resolve(target, ref target.Comp))
            return false;

        var damageableDict = target.Comp.Damage.DamageDict;
        var healingDict = healing.Comp.HealAmount.DamageDict;
        foreach (var type in healingDict)
        {
            if (damageableDict[type.Key].Value > 0)
            {
                return true;
            }
        }

        return false;
    }

    private void OnMendAction(Entity<SpiderMenderComponent> ent, ref OnSpiderMendActionEvent args)
    {
        if (_whitelist.IsWhitelistFail(ent.Comp.Whitelist, args.Target) || args.Handled)
            return;

        if (!_energy.CanSpendEnergy(ent.Owner, ent.Comp.EnergyName, ent.Comp.HealCost))
        {
            var message = Loc.GetString("spider-mend-fail-energy");
            _popup.PopupClient(message, ent, PopupType.SmallCaution);
            return;
        }

        if (!HasDamage(ent, args.Target))
            return;

        var time = ent.Comp.HealTime;

        if (ent.Owner == args.Target)
            time = ent.Comp.HealTime * 2;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, time, new SpiderMendDoAfterEvent(), ent, args.Target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
        });

        if (ent.Owner == args.Target)
        {
            var selfMessage = Loc.GetString("spider-mend-start-self", ("spider", ent.Owner));
            _popup.PopupClient(
                selfMessage,
                ent,
                ent,
                PopupType.Medium);
            return;
        }

        var othersMessage = Loc.GetString("spider-mend-start-others", ("spider", ent.Owner));
        _popup.PopupEntity(
            othersMessage,
            args.Target,
            args.Target,
            PopupType.Medium);
    }

    private void OnSpiderMendDoAfter(Entity<SpiderMenderComponent> ent, ref SpiderMendDoAfterEvent args)
    {
        if (args.Cancelled | args.Handled)
            return;

        if (args.Target == null)
            return;

        if (!_energy.TrySpendEnergy(ent.Owner, ent.Comp.EnergyName, ent.Comp.HealCost))
            return;

        _bloodstreamSystem.TryModifyBleedAmount(args.Target.Value, ent.Comp.BloodlossModifier);
        _damage.TryChangeDamage(args.Target.Value, ent.Comp.HealAmount);
    }
}
