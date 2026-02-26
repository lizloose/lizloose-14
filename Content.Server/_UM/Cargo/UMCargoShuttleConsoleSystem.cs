using System.Diagnostics.CodeAnalysis;
using Content.Server._UM.Cargo.Events;
using Content.Server.Cargo.Systems;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Shuttles.Systems;
using Content.Shared._UM.Cargo.Components;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Systems;
using Content.Shared.Station;
using Content.Shared.Timing;
using Robust.Server.GameObjects;

namespace Content.Server._UM.Cargo;

/// <summary>
/// This handles...
/// </summary>
public sealed class UMCargoShuttleConsoleSystem : EntitySystem
{
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly SharedStationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly CargoSystem _cargoSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UMCargoShuttleConsoleComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<UMCargoShuttleConsoleComponent, UMSendCargoShuttleMessage>(OnCargoRequestSend);

        SubscribeLocalEvent<UMCargoShuttleComponent, FTLStartedEvent>(OnFTLStart);
        SubscribeLocalEvent<UMCargoShuttleComponent, FTLCompletedEvent>(OnFTLComplete);
        SubscribeLocalEvent<UMCargoShuttleComponent, FTLAvailableEvent>(OnFTLAvailable);
    }

    private void OnMapInit(Entity<UMCargoShuttleConsoleComponent> ent, ref MapInitEvent args)
    {
        var query = EntityQueryEnumerator<UMCargoShuttleComponent>();
        var owningStation = _station.GetOwningStation(ent.Owner);

        while (query.MoveNext(out var uid, out var shuttleComp))
        {
            if (_station.GetOwningStation(uid) == owningStation)
            {
                ent.Comp.ShuttleUid = uid;
                return;
            }
        }
    }

    private void OnCargoRequestSend(Entity<UMCargoShuttleConsoleComponent> ent, ref UMSendCargoShuttleMessage args)
    {
        if (!TryComp<ShuttleComponent>(ent.Comp.ShuttleUid, out var shuttleComp))
            return;

        MoveCargoShuttle((ent.Comp.ShuttleUid, shuttleComp));
    }

    private void UpdateUi(Entity<UMCargoShuttleConsoleComponent?> ent, FTLState ftlState, StartEndTime? time)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        if (!_userInterface.HasUi(ent.Owner, UMCargoShuttleUiKey.Key))
            return;

        var shuttleLocation = GetShuttleLocation(ent.Comp.ShuttleUid);

        var uiState = new UMCargoShuttleBoundUserInterfaceState(ftlState, time, GetNetEntity(shuttleLocation));
        _userInterface.SetUiState(ent.Owner, UMCargoShuttleUiKey.Key, uiState);
    }

    public void MoveCargoShuttle(Entity<ShuttleComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        if (TryComp<FTLComponent>(ent.Owner, out var ftlComponent) && ftlComponent.State != FTLState.Available)
            return;

        var shuttleXform = Transform(ent.Owner);

        var stationUid = _station.GetOwningStation(ent.Owner);
        if (!stationUid.HasValue)
            return;

        var stationGrid = _station.GetLargestGrid(stationUid.Value);
        if (!stationGrid.HasValue)
            return;

        if (shuttleXform.MapUid != Transform(stationGrid.Value).MapUid)
        {
            _shuttle.FTLToDock(ent.Owner, ent.Comp, stationGrid.Value);
            return;
        }

        if (!TryComp<StationCentcommComponent>(stationUid, out var centcomm))
            return;

        if (centcomm.Entity == null)
            return;

        _shuttle.FTLToDock(ent.Owner, ent.Comp, centcomm.Entity.Value);
    }

    private void UpdateCargoShuttleConsoles(EntityUid shuttleUid, FTLState ftlState, StartEndTime? time)
    {
        var query = EntityQueryEnumerator<UMCargoShuttleConsoleComponent>();

        while (query.MoveNext(out var uid, out var shuttleComp))
        {
            if (shuttleComp.ShuttleUid == shuttleUid)
            {
                UpdateUi(uid, ftlState, time);
            }
        }
    }

    private void OnFTLStart(Entity<UMCargoShuttleComponent> ent, ref FTLStartedEvent args)
    {
        if (!TryComp<FTLComponent>(ent, out var ftlComponent))
            return;

        UpdateCargoShuttleConsoles(ent.Owner, ftlComponent.State, ftlComponent.StateTime);
    }

    private void OnFTLComplete(Entity<UMCargoShuttleComponent> ent, ref FTLCompletedEvent args)
    {
        if (!TryComp<FTLComponent>(ent, out var ftlComponent))
            return;

        UpdateCargoShuttleConsoles(ent.Owner, ftlComponent.State, ftlComponent.StateTime);
        }

    private void OnFTLAvailable(Entity<UMCargoShuttleComponent> ent, ref FTLAvailableEvent args)
    {
        UpdateCargoShuttleConsoles(ent.Owner, FTLState.Available, null);

        //Sell once we can ftl back :)
        var shuttleXform = Transform(ent.Owner);
        var stationUid = _station.GetOwningStation(ent.Owner);
        if (stationUid == null || shuttleXform.MapUid == null || shuttleXform.GridUid == null)
            return;

        if (!TryComp<StationCentcommComponent>(stationUid, out var centcomm) || centcomm.MapEntity == null)
            return;

        if (shuttleXform.MapUid != centcomm.MapEntity)
            return;

        _cargoSystem.SellPallets(shuttleXform.GridUid.Value, stationUid.Value, out var sold);

    }

    private EntityUid? GetShuttleLocation(EntityUid shuttleUid)
    {
        return Transform(shuttleUid).MapUid;
    }

}
