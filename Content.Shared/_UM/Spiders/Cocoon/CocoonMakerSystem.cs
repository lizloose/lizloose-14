using Content.Shared._UM.Energy;
using Content.Shared.Actions;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Random;

namespace Content.Shared._UM.Spiders.Cocoon;

/// <summary>
/// This handles...
/// </summary>
public sealed class CocoonMakerSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly EnergyContainerSystem _energy = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CocoonMakerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CocoonMakerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CocoonMakerComponent, OnCocoonWrapActionEvent>(OnCocoonWrapAction);
        SubscribeLocalEvent<CocoonMakerComponent, OnCocoonWrapDoAfterEvent>(OnCocoonWrapDoAfter);
        SubscribeLocalEvent<CocoonMakerComponent, OnCocoonEnergyAbsorbDoAfterEvent>(OnCocoonEnergyAbsorbDoAfter);
    }

    private void OnMapInit(Entity<CocoonMakerComponent> ent, ref MapInitEvent args)
    {
        _actionsSystem.AddAction(ent, ref ent.Comp.CocoonActionEntity, ent.Comp.CocoonAction);
    }

    private void OnShutdown(Entity<CocoonMakerComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.CocoonActionEntity != null)
        {
            _actionsSystem.RemoveAction(ent.Owner, ent.Comp.CocoonActionEntity);
        }
    }

    private void OnCocoonWrapAction(Entity<CocoonMakerComponent> ent, ref OnCocoonWrapActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<MobStateComponent>(args.Target, out var mobState) || !HasComp<MobSizeComponent>(args.Target))
            return;

        if (mobState.CurrentState == MobState.Alive)
            return;

        var selfMessage = Loc.GetString("spider-start-cocooning-self", ("target", args.Target));
        var othersMessage = Loc.GetString("spider-start-cocooning-others", ("spider", ent.Owner), ("target", args.Target));
        _popup.PopupPredicted(
            selfMessage,
            othersMessage,
            ent,
            ent,
            PopupType.LargeCaution);

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, ent.Comp.WrapTime, new OnCocoonWrapDoAfterEvent(), ent, args.Target)
        {
            BreakOnMove = true,
            NeedHand = false,
        });

        args.Handled = true;
    }

    private void OnCocoonWrapDoAfter(Entity<CocoonMakerComponent> ent, ref OnCocoonWrapDoAfterEvent args)
    {
        if (_net.IsClient || args.Target == null || args.Cancelled)
            return;

        if (!TryComp<MobSizeComponent>(args.Target.Value, out var mobSizeComponent))
            return;

        var position = Transform(args.Target.Value).Coordinates;

        var size = ent.Comp.LargeCocoon;
        if (mobSizeComponent.Size == MobSizes.Small)
            size = ent.Comp.SmallCocoon;

        var cocoon = Spawn(size, position);

        if (!TryComp<CocoonComponent>(cocoon, out var cocoonComponent))
            return;

        _container.Insert(args.Target.Value, cocoonComponent.Contents);
    }

    private void OnCocoonEnergyAbsorbDoAfter(Entity<CocoonMakerComponent> ent, ref OnCocoonEnergyAbsorbDoAfterEvent args)
    {
        if (args.Cancelled || args.Target == null)
            return;

        if (!TryComp<CocoonComponent>(args.Target, out var cocoon) || cocoon.Harvested)
            return;

        Dirty(ent);
        cocoon.Harvested = true;

        var energy = _random.Next(cocoon.MinEnergy, cocoon.MaxEnergy);

        _energy.TryAddEnergy(ent.Owner, ent.Comp.EnergyName, energy);

        foreach (var mob in cocoon.Contents.ContainedEntities)
        {
            _damage.TryChangeDamage(mob, ent.Comp.Damage, origin: ent);
        }
    }
}
