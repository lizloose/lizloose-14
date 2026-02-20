using Content.Shared._UM.Changeling.Devour.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.Armor;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Changeling.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Changeling.Devour;

/// <summary>
/// Mostly a copy and paste of upstream because I don't want merge conflicts.
/// </summary>
public sealed class UMChangelingDevourActionSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly UMSharedChangelingSystem _changeling = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private static readonly ProtoId<DamageTypePrototype> CellularDamageType = "Cellular";

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<UMChangelingDevourActionComponent, UMChangelingDevourActionEvent>(OnDevourAction);
        SubscribeLocalEvent<ChangelingIdentityComponent, UMChangelingDevourWindupDoAfterEvent>(OnDevourWindup);
        SubscribeLocalEvent<ChangelingIdentityComponent, UMChangelingDevourConsumeDoAfterEvent>(OnDevourConsume);
    }

    private bool IsTargetProtected(EntityUid target, List<ProtoId<DamageTypePrototype>> damageTypes, float damageThreshold)
    {
        var ev = new CoefficientQueryEvent(SlotFlags.OUTERCLOTHING);

        RaiseLocalEvent(target, ev);

        foreach (var compProtectiveDamageType in damageTypes)
        {
            if (!ev.DamageModifiers.Coefficients.TryGetValue(compProtectiveDamageType, out var coefficient))
                continue;
            if (coefficient < 1f - damageThreshold)
                return true;
        }

        return false;
    }

    private void OnDevourAction(Entity<UMChangelingDevourActionComponent> ent,
        ref UMChangelingDevourActionEvent args)
    {
        if (args.Handled || _whitelistSystem.IsWhitelistFailOrNull(ent.Comp.Whitelist, args.Target)
                         || !HasComp<ChangelingIdentityComponent>(args.Performer))
            return;

        args.Handled = true;

        var target = args.Target;
        var performer = args.Performer;

        if (target == performer)
            return;

        if (_mobState.IsAlive(target))
        {
            _popupSystem.PopupClient(Loc.GetString("changeling-devour-attempt-failed-alive"), performer, performer, PopupType.Medium);
            return;
        }

        if (HasComp<RottingComponent>(target))
        {
            _popupSystem.PopupClient(Loc.GetString("changeling-devour-attempt-failed-rotting"), args.Performer, args.Performer, PopupType.Medium);
            return;
        }

        if (IsTargetProtected(target, ent.Comp.ProtectiveDamageTypes, ent.Comp.DevourPreventionPercentageThreshold))
        {
            _popupSystem.PopupClient(Loc.GetString("changeling-devour-attempt-failed-protected"), ent, ent, PopupType.Medium);
            return;
        }

        if (!_changeling.CanExtractDna(performer, target))
        {
            _popupSystem.PopupClient(Loc.GetString("changeling-extract-genome-sting-already-absorbed", ("target", target)), performer, performer);
            return;
        }

        if (_net.IsServer)
        {
            var pvsSound = _audio.PlayPvs(ent.Comp.DevourWindupNoise, ent);
            if (pvsSound != null)
                ent.Comp.CurrentDevourSound = pvsSound.Value.Entity;
        }

        _adminLogger.Add(LogType.Action, LogImpact.Medium, $"{ent:player} started changeling devour windup against {target:player}");

        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, performer, ent.Comp.DevourWindupTime, new UMChangelingDevourWindupDoAfterEvent(ent.Comp.DevourConsumeTime, ent.Comp.ConsumeNoise), performer, target: target)
        {
            BreakOnMove = true,
            BlockDuplicate = true,
            DuplicateCondition = DuplicateConditions.None,
        });

        var selfMessage = Loc.GetString("changeling-devour-begin-windup-self", ("user", Identity.Entity(ent.Owner, EntityManager)));
        var othersMessage = Loc.GetString("changeling-devour-begin-windup-others", ("user", Identity.Entity(ent.Owner, EntityManager)));
        _popupSystem.PopupPredicted(
            selfMessage,
            othersMessage,
            args.Performer,
            args.Performer,
            PopupType.MediumCaution);
    }

    private void OnDevourWindup(Entity<ChangelingIdentityComponent> ent,
        ref UMChangelingDevourWindupDoAfterEvent args)
    {
        args.Handled = true;
        if (args.Cancelled)
            return;

        var selfMessage = Loc.GetString("changeling-devour-begin-consume-self", ("user", Identity.Entity(ent.Owner, EntityManager)));
        var othersMessage = Loc.GetString("changeling-devour-begin-consume-others", ("user", Identity.Entity(ent.Owner, EntityManager)));
        _popupSystem.PopupPredicted(
            selfMessage,
            othersMessage,
            args.User,
            args.User,
            PopupType.LargeCaution);

        if (_net.IsServer)
            _audio.PlayPvs(args.ConsumeNoise, ent);

        _adminLogger.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(ent.Owner):player} began to devour {ToPrettyString(args.Target):player} identity");

        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager,
            ent,
            args.DevourConsumeTime,
            new UMChangelingDevourConsumeDoAfterEvent(),
            ent,
            target: args.Target,
            used: ent)
        {
            AttemptFrequency = AttemptFrequency.EveryTick,
            BreakOnMove = true,
            BlockDuplicate = true,
            DuplicateCondition = DuplicateConditions.None,
        });
    }

    private void OnDevourConsume(Entity<ChangelingIdentityComponent> ent,
        ref UMChangelingDevourConsumeDoAfterEvent args)
    {
        args.Handled = true;
        var target = args.Target;

        if (target == null)
            return;

        if (args.Cancelled)
            return;

        if (_mobState.IsAlive(target.Value))
        {
            _adminLogger.Add(LogType.Action,
                LogImpact.Medium,
                $"{ToPrettyString(ent.Owner):player}  unsuccessfully devoured {ToPrettyString(args.Target):player}'s identity");
            _popupSystem.PopupClient(Loc.GetString("changeling-devour-consume-failed-not-dead"),
                args.User,
                args.User,
                PopupType.Medium);
            return;
        }

        if (!_changeling.TryExtractDna((ent, ent.Comp), target.Value))
            return;

        var selfMessage = Loc.GetString("changeling-devour-consume-complete-self", ("user", Identity.Entity(args.User, EntityManager)));
        var othersMessage = Loc.GetString("changeling-devour-consume-complete-others", ("user", Identity.Entity(args.User, EntityManager)));
        _popupSystem.PopupPredicted(
            selfMessage,
            othersMessage,
            args.User,
            args.User,
            PopupType.LargeCaution);

        _changeling.TryAddStorePoints(ent.Owner, 15); //TODO: Don't hardcode this

        _damageable.TryChangeDamage(target.Value, new DamageSpecifier(_prototype.Index(CellularDamageType), 300));

    }
}
