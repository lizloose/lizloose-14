using Content.Shared._UM.Drip.Components;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server._UM.Drip;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class DripDrobeSystem : EntitySystem
{
    [Dependency] private DripTrackingManager _dripTracking = default!;
    [Dependency] private ISharedPlayerManager _playerManager = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DripDrobeComponent, DripDrobeDispenseItemMessage>(OnRequestDispense);
    }

    private void OnRequestDispense(Entity<DripDrobeComponent> ent, ref DripDrobeDispenseItemMessage args)
    {
        if (!_playerManager.TryGetSessionByEntity(args.Actor, out var session))
            return;

        var availableEntities = _dripTracking.GetAvailableEntities(session);

        if (!availableEntities.TryGetValue(args.Item.Id, out var amount) || amount <= 0)
            return;

        var item = SpawnNextToOrDrop(args.Item.Id, ent.Owner);
        if (TryComp<TrackedDripComponent>(item, out var drip))
        {
            drip.Spent = true;
            Dirty(item, drip);
        }

        _hands.TryForcePickupAnyHand(args.Actor, item);

        _dripTracking.SetDripRounds(session, args.Item.Id, amount - 1);
        _audio.PlayPvs(ent.Comp.SoundVend, ent.Owner);
    }
}
