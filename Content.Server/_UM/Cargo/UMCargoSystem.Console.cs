using Content.Server.Shuttles.Components;
using Content.Shared._UM.Cargo.Components;
using Content.Shared.Shuttles.Systems;
using Content.Shared.Timing;

namespace Content.Server._UM.Cargo;

public sealed partial class UMCargoSystem
{
    private void InitializeConsole()
    {
        SubscribeLocalEvent<UMCargoShuttleConsoleComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<UMCargoShuttleConsoleComponent, UMSendCargoShuttleMessage>(OnCargoRequestSend);
    }

    private void OnMapInit(Entity<UMCargoShuttleConsoleComponent> ent, ref MapInitEvent args)
    {
        var query = EntityQueryEnumerator<UMCargoShuttleComponent>();
        var owningStation = _station.GetOwningStation(ent.Owner);

        while (query.MoveNext(out var uid, out _))
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
}
