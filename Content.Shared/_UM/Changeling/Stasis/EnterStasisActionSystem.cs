using Content.Shared._UM.Changeling.Stasis.Components;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Administration.Systems;
using Content.Shared.Alert;
using Content.Shared.Ghost;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared._UM.Changeling.Stasis;

/// <summary>
/// This handles...
/// </summary>
public sealed class EnterStasisActionSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EnterStasisActionComponent, StasisEnterActionEvent>(OnStasisEnterAction);
        SubscribeLocalEvent<RevivingStasisComponent, StasisReviveAlertEvent>(OnStasisReviveAlert);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<RevivingStasisComponent>();

        // Loop over all entities.
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.StasisFinished)
                continue;

            if (comp.StasisEnd > curTime)
                continue;

            StasisReady((uid, comp));
        }
    }

    private void OnStasisEnterAction(Entity<EnterStasisActionComponent> ent, ref StasisEnterActionEvent args)
    {
        if (args.Handled)
            return;

        if (HasComp<RevivingStasisComponent>(ent))
            return;

        if (_mobState.IsAlive(args.Performer))
        {
            var othersMessage = Loc.GetString("suicide-command-default-text-others", ("name", Identity.Entity(args.Performer, EntityManager)));
            _popup.PopupPredicted(othersMessage, args.Performer, null, Filter.PvsExcept(args.Performer), true);
        }

        var selfMessage = Loc.GetString("changeling-enter-stasis-message");
        _popup.PopupClient(selfMessage, args.Performer, PopupType.LargeCaution);

        if (!_mobState.IsDead(args.Performer))
            _mobState.ChangeMobState(args.Performer, MobState.Dead);

        var stasisComp = EnsureComp<RevivingStasisComponent>(args.Performer);
        stasisComp.StasisEnd = _timing.CurTime + stasisComp.Duration;
        stasisComp.ActionEntity = args.Action;

        if (RemComp<GhostOnMoveComponent>(args.Performer))
            stasisComp.GhostOnMove = true;

        _alerts.ShowAlert(args.Performer, stasisComp.StasisAlert);

        args.Handled = true;
    }

    private void StasisReady(Entity<RevivingStasisComponent> ent)
    {
        _alerts.ClearAlert(ent.Owner, ent.Comp.StasisAlert);
        _alerts.ShowAlert(ent.Owner, ent.Comp.StasisReadyAlert);
        ent.Comp.StasisFinished = true;
    }

    private void OnStasisReviveAlert(Entity<RevivingStasisComponent> ent, ref StasisReviveAlertEvent args)
    {
        _alerts.ClearAlert(ent.Owner, ent.Comp.StasisReadyAlert);
        _rejuvenate.PerformRejuvenate(ent);

        //Because rejuvenate and cooldowns grr
        if (ent.Comp.ActionEntity != null &&
            TryComp<ActionComponent>(ent.Comp.ActionEntity.Value, out var actionComp) &&
            actionComp.UseDelay != null)
            _actions.SetCooldown(ent.Comp.ActionEntity.Value, actionComp.UseDelay.Value);

        if (ent.Comp.GhostOnMove)
        {
            var ghost = AddComp<GhostOnMoveComponent>(ent);
            ghost.CanReturn = false;
            ghost.MustBeDead = true;
        }

        RemComp<RevivingStasisComponent>(ent);
    }
}
