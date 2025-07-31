using System.Numerics;
using Content.Server.Parallax;
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
public sealed class PlanetSystem : EntitySystem
{

    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly BiomeSystem _biomes = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private readonly List<(Vector2i, Tile)> _mapTiles = new();

    /// <inheritdoc/>
    public override void Initialize()
    {
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

        //Ore time
        var biomeComp = EnsureComp<BiomeComponent>(mapUid);
        foreach (var layer in ent.Comp.OreLayers)
        {
            _biomes.AddMarkerLayer(mapUid, biomeComp, layer);
        }

        //TODO: Replace with the Ferry Shuttles
        AddComp<FTLDestinationComponent>(mapUid);

        //Loading the station grid
        if (!_mapLoader.TryLoadGrid(mapId, ent.Comp.Path, out var grid))
            return;

        //Ensuring no overlap of station and planet rocks
        _mapTiles.Clear();
        var bounds = Comp<MapGridComponent>(grid.Value).LocalAABB;
        _biomes.ReserveTiles(mapUid, bounds, _mapTiles);

        if (!_protoManager.TryIndex(ent.Comp.GridPool,  out var gridPool))
            return;


        var gridsCount = gridPool.Grids.Count;


        foreach (var room in gridPool.Grids)
        {
            var coordinates = new Vector2(_random.Next(30, gridPool.Distance), _random.Next(30, gridPool.Distance));

            if ( _random.Next(1, 100) > 50)
            {
                coordinates.X *= -1;
            }
            var negative = _random.Next(1, 100);
            if ( _random.Next(1, 100) > 50)
            {
                coordinates.Y *= -1;
            }

            if (!_mapLoader.TryLoadGrid(mapId, _protoManager.Index(room).Path, out var roomGrid))
                continue;

            _transform.SetCoordinates(roomGrid.Value, new EntityCoordinates(mapUid, coordinates.X, coordinates.Y));
            var roomBounds = Comp<MapGridComponent>(roomGrid.Value).LocalAABB;
        }

        _map.InitializeMap(mapUid);
    }
}
