using System.Numerics;
using Content.Server.Parallax;
using Content.Server.Procedural;
using Content.Server.Station.Events;
using Content.Server.Worldgen.Tools;
using Content.Shared.Interaction;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Shuttles.Components;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Planets;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class PlanetSystem : EntitySystem
{

    private EntityQuery<MetaDataComponent> _metaQuery;

    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly BiomeSystem _biomes = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PoissonDiskSampler _sampler = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    private readonly List<(Vector2i, Tile)> _mapTiles = new();

    /// <inheritdoc/>
    public override void Initialize()
    {
        _metaQuery = GetEntityQuery<MetaDataComponent>();
        SubscribeLocalEvent<PlanetSpawnComponent, StationPostInitEvent>(OnPlanetSpawnPostInit);
    }

    private void OnPlanetSpawnPostInit(Entity<PlanetSpawnComponent> ent, ref StationPostInitEvent args)
    {
        PlanetSpawn(ent);
    }

    private void PlanetSpawn(Entity<PlanetSpawnComponent> ent)
    {
        _map.CreateMap(out var mapId, false);
        var mapUid = _map.GetMapOrInvalid(mapId);

        _biomes.EnsurePlanet(mapUid, _protoManager.Index(ent.Comp.Biome));

        if (!TryComp<MapGridComponent>(mapUid, out var mapGrid))
            return;

        //Ore time

        var biomeComp = EnsureComp<BiomeComponent>(mapUid);
        foreach (var layer in ent.Comp.OreLayers)
        {
            _biomes.AddMarkerLayer(mapUid, biomeComp, layer);
        }

        //TODO: Replace with the Ferry Shuttles
        AddComp<FTLDestinationComponent>(mapUid);

        if (!_mapLoader.TryLoadGrid(mapId, ent.Comp.Path, out var stationGrid))
            return;
        _transform.SetCoordinates(stationGrid.Value, new EntityCoordinates(mapUid, 0, 0));

        _map.InitializeMap(mapUid);

        if (!_protoManager.TryIndex(ent.Comp.GridPool,  out var gridPool))
            return;

        var debrisPoints = GeneratePoints();
        _random.Shuffle(debrisPoints);

        var rotations = new List<int> {0, 90, 180, 270};

        var worldRotation = _transform.GetWorldRotation(mapUid);

        foreach (var room in gridPool.Grids)
        {
            var originTransform = Matrix3Helpers.CreateTranslation(0f, 0f);

            var coords = _random.PickAndTake(debrisPoints);
            Log.Debug("new coords" + coords.ToString());

            LoadGrid(mapUid, mapGrid, coords, 0f, _protoManager.Index(room).Path);

            //var coordinates = Vector2.Round(_random.PickAndTake(debrisPoints));
            //if (!_mapLoader.TryLoadGrid(mapId, _protoManager.Index(room).Path, out var roomGrid))
            //    continue;
            //_transform.SetCoordinates(roomGrid.Value, new EntityCoordinates(mapUid, coordinates.X, coordinates.Y));
        }
    }

    private List<Vector2i> GeneratePoints()
    {
        var topLeft = new Vector2i(-100, -100);
        var lowerRight = new Vector2i(100, 100);
        var enumerator = _sampler.SampleRectangle(topLeft, lowerRight, 30f);

        var gridPoints = new List<Vector2i>();

        while (enumerator.MoveNext(out var debrisPoint))
        {
            gridPoints.Add(debrisPoint.Value.Ceiled());
        }

        return gridPoints;
    }

}
