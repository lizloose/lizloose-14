using Content.Server._UM.Cargo.Events;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Shared._UM.Cargo.Components;
using Content.Shared.Popups;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Systems;

namespace Content.Server._UM.Cargo;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class UMCargoSystem
{

    private void InitializeShuttle()
    {
        SubscribeLocalEvent<UMCargoShuttleComponent, FTLStartedEvent>(OnFTLStart);
        SubscribeLocalEvent<UMCargoShuttleComponent, FTLCompletedEvent>(OnFTLComplete);
        SubscribeLocalEvent<UMCargoShuttleComponent, FTLAvailableEvent>(OnFTLAvailable);
        SubscribeLocalEvent<UMCargoShuttleComponent, FTLTagEvent>(OnShuttleTag);
    }

    private void MoveCargoShuttle(Entity<ShuttleComponent?> ent, Entity<UMCargoShuttleConsoleComponent> consoleEnt)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        if (!TryComp<UMCargoShuttleComponent>(ent.Owner, out var cargoShuttleComponent))
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

        if (cargoShuttleComponent.FirstWarp)
        {
            FTLCargoShuttle(ent.Owner, ent.Comp, stationGrid.Value, true);
            cargoShuttleComponent.FirstWarp = false;
            return;
        }

        if (shuttleXform.MapUid != Transform(stationGrid.Value).MapUid)
        {
            FTLCargoShuttle(ent.Owner, ent.Comp, stationGrid.Value, true);
            cargoShuttleComponent.DumpMobs = false;
            return;
        }

        if (!TryComp<StationCentcommComponent>(stationUid, out var centcomm))
            return;

        if (centcomm.Entity == null)
            return;

        if (!FTLCargoShuttle(ent.Owner, ent.Comp, centcomm.Entity.Value, false))
        {
            _popup.PopupEntity("Please remove all life forms", consoleEnt.Owner, PopupType.SmallCaution);
            PlayDenySound(consoleEnt);
        }
    }

    private bool FTLCargoShuttle(EntityUid shuttleUid, ShuttleComponent component, EntityUid target, bool allowMobs)
    {
        if (!allowMobs && _UMShuttle.HasDumpChildren(shuttleUid))
            return false;

        _shuttle.FTLToDock(shuttleUid, component, target, priorityTag: "DockCargo");

        if (TryComp<FTLComponent>(shuttleUid, out var ftlComponent))
            UpdateCargoShuttleConsoles(shuttleUid, ftlComponent.State, ftlComponent.StateTime);

        return true;
    }

    private void OnFTLStart(Entity<UMCargoShuttleComponent> ent, ref FTLStartedEvent args)
    {
        if (ent.Comp.DumpMobs)
            _UMShuttle.DumpChildren(ent.Owner, ref args);

        ent.Comp.DumpMobs = true;
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

        PalletSell(ent);
        TryFulfillOrders(ent);
    }

    private void OnShuttleTag(Entity<UMCargoShuttleComponent> ent, ref FTLTagEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        args.Tag = "DockCargo";
    }

    private EntityUid? GetShuttleLocation(EntityUid shuttleUid)
    {
        return Transform(shuttleUid).MapUid;
    }
}
