using Content.Server.Shuttles.Systems;
using Content.Shared._UM.Drip;
using Content.Shared._UM.Drip.Components;
using Content.Shared.GameTicking;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Server.Player;
using Robust.Shared.Containers;
using Robust.Shared.Player;

namespace Content.Server._UM.Drip;

/// <inheritdoc/>
public sealed partial class TrackedDripSystem : SharedTrackedDripSystem
{
    [Dependency] private EmergencyShuttleSystem _eShuttle = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedTransformSystem _xform = default!;
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private DripTrackingManager _dripTracking = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundEndMessageEvent>(OnRoundEnd);
    }

    private void OnRoundEnd(RoundEndMessageEvent ev)
    {
        var dripQuery = EntityQueryEnumerator<TrackedDripComponent>();

        while (dripQuery.MoveNext(out var uid, out var comp))
        {
            TrackTheDrip((uid, comp));
        }
    }

    private void TrackTheDrip(Entity<TrackedDripComponent> ent)
    {
        if (ent.Comp.Spent)
            return;

        var prototype = MetaData(ent.Owner).EntityPrototype;
        if (prototype == null)
            return;

        if (!_container.TryGetOuterContainer(ent, Transform(ent), out var container))
            return;

        var player = container.Owner;

        if (!_playerManager.TryGetSessionByEntity(player, out var session))
            return;

        if (!TryComp<MobStateComponent>(player, out var mobState) || mobState.CurrentState == MobState.Dead)
            return;

        if (ent.Comp.RequireShuttle)
        {
            var shuttle = _eShuttle.GetShuttle();
            if (shuttle == null)
                return;
            if (Transform(shuttle.Value).MapID != _xform.GetMapCoordinates(player).MapId)
                return;
        }

        UpdateDrip(session, prototype.ID, ent.Comp.Rounds);
    }

    public void UpdateDrip(ICommonSession playerId, string dripId, int rounds)
    {
        _dripTracking.SetDripRounds(playerId, dripId, rounds);
    }
}
