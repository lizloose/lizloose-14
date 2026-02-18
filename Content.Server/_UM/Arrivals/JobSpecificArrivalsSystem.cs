using System.Numerics;
using Content.Server._UM.Arrivals.Components;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Shuttles.Systems;
using Content.Server.Spawners.Components;
using Content.Server.Spawners.EntitySystems;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Content.Shared.CCVar;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Shuttles.Components;
using Content.Shared.Station;
using Content.Shared.Tag;
using Content.Shared.Tiles;
using Robust.Shared.Configuration;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._UM.Arrivals;

public sealed class TiderArrivalsSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedStationSystem _station = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ActorSystem _actor = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    private bool _arrivalsEnabled = true;

    private static readonly ProtoId<TagPrototype> DockTagProto = "DockEmergency";

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StationJobArrivalsComponent, StationPostInitEvent>(OnStationPostInit);

        SubscribeLocalEvent<ShuttleJobArrivalsComponent, FTLStartedEvent>(OnFTLStarted);
        SubscribeLocalEvent<ShuttleJobArrivalsComponent, FTLCompletedEvent>(OnFTLCompleted);

        SubscribeLocalEvent<ShuttleJobArrivalsComponent, FTLTagEvent>(OnShuttleTag);

        SubscribeLocalEvent<PlayerSpawningEvent>(HandlePlayerSpawning, before: [typeof(SpawnPointSystem)]);

        _config.OnValueChanged(CCVars.ArrivalsShuttles, OnArrivalsConfigChanged, true);
    }

    private void OnArrivalsConfigChanged(bool val)
    {
        if (_arrivalsEnabled && !val && _gameTicker.RunLevel != GameRunLevel.PreRoundLobby)
            return;

        _arrivalsEnabled = val;
    }

    private void OnStationPostInit(Entity<StationJobArrivalsComponent> ent, ref StationPostInitEvent args)
    {
        if (!_arrivalsEnabled)
            return;
        SetupShuttle(ent);
    }

    private void OnShuttleTag(Entity<ShuttleJobArrivalsComponent> ent, ref FTLTagEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        args.Tag = DockTagProto;
    }

    private void OnFTLStarted(Entity<ShuttleJobArrivalsComponent> ent, ref FTLStartedEvent args)
    {
        if (!TryComp<StationJobArrivalsComponent>(ent.Comp.Station, out var station))
            return;

        if (station.Docked)
        {
            DumpChildren(ent.Owner, ref args);
            QueueDel(ent);
            Log.Debug("Deleted Arrivals Shuttle");
        }
    }


    private void OnFTLCompleted(Entity<ShuttleJobArrivalsComponent> ent, ref FTLCompletedEvent args)
    {
        if (!TryComp<StationJobArrivalsComponent>(ent.Comp.Station, out var station))
            return;

        if (station.Docked)
            return;

        station.Docked = true;
    }


    private void DumpChildren(EntityUid uid, ref FTLStartedEvent args)
    {
        var toDump = new List<Entity<TransformComponent>>();
        FindDumpChildren(uid, toDump);
        foreach (var (ent, xform) in toDump)
        {
            var rotation = xform.LocalRotation;
            _transform.SetCoordinates(ent, new EntityCoordinates(args.FromMapUid!.Value, Vector2.Transform(xform.LocalPosition, args.FTLFrom)));
            _transform.SetWorldRotation(ent, args.FromRotation + rotation);
            if (_actor.TryGetSession(ent, out var session))
            {
                _chat.DispatchServerMessage(session!, Loc.GetString("latejoin-arrivals-dumped-from-shuttle"));
            }
        }
    }
    private void FindDumpChildren(EntityUid uid, List<Entity<TransformComponent>> toDump)
    {
        var xform = Transform(uid);

        if (HasComp<MobStateComponent>(uid) || HasComp<ArrivalsBlacklistComponent>(uid))
        {
            toDump.Add((uid, xform));
            return;
        }

        var children = xform.ChildEnumerator;
        while (children.MoveNext(out var child))
        {
            FindDumpChildren(child, toDump);
        }
    }

    public void HandlePlayerSpawning(PlayerSpawningEvent ev)
    {
        if (ev.SpawnResult != null)
            return;

        if (!TryComp<StationJobArrivalsComponent>(ev.Station, out var arrivals) || arrivals.ShuttleUid is not { } grid)
            return;

        if (arrivals.Docked)
            return;

        if (ev.Job != arrivals.Job)
            return;

        var points = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
        var possiblePositions = new List<EntityCoordinates>();
        while (points.MoveNext(out _, out var spawnPoint, out var xform))
        {
            if (xform.GridUid != grid)
                continue;

            possiblePositions.Add(xform.Coordinates);
        }

        if (possiblePositions.Count <= 0)
            return;

        var spawnLoc = _random.Pick(possiblePositions);
        ev.SpawnResult = _stationSpawning.SpawnPlayerMob(
            spawnLoc,
            ev.Job,
            ev.HumanoidCharacterProfile,
            ev.Station);

        EnsureComp<PendingClockInComponent>(ev.SpawnResult.Value);
        EnsureComp<AutoOrientComponent>(ev.SpawnResult.Value);
    }

    public void SetupShuttle(Entity<StationJobArrivalsComponent> ent)
    {
        if (ent.Comp.ShuttleUid != null)
            return;

        _map.CreateMap(out var mapId);

        if (!_mapLoader.TryLoadGrid(mapId, ent.Comp.ShuttlePath, out var shuttle))
            return;

        _shuttle.TryFTLProximity(shuttle.Value, _shuttle.EnsureFTLMap());

        ent.Comp.ShuttleUid = shuttle.Value;

        var arrivalsComp = EnsureComp<ShuttleJobArrivalsComponent>(shuttle.Value);
        arrivalsComp.Station = ent;
        EnsureComp<ProtectedGridComponent>(shuttle.Value);
        EnsureComp<PreventPilotComponent>(shuttle.Value);

        ResetTimer((shuttle.Value, arrivalsComp));

        _map.DeleteMap(mapId);
    }

    private void ResetTimer(Entity<ShuttleJobArrivalsComponent> ent)
    {
        if (_station.GetLargestGrid(ent.Comp.Station) is not { } grid)
            return;

        _shuttle.FTLToDock(ent, Comp<ShuttleComponent>(ent), grid, hyperspaceTime: (float) ent.Comp.FlightDelay.TotalSeconds);
        ent.Comp.TakeoffTime = _timing.CurTime + ent.Comp.FlightDelay + ent.Comp.RestDelay;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ShuttleJobArrivalsComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.TakeoffTime)
                continue;
            ResetTimer((uid, comp));
        }
    }
}
