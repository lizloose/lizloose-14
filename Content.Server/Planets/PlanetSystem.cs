using System.Numerics;
using Content.Server.Parallax;
using Content.Server.Station.Events;
using Content.Shared.Interaction;
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

        AddComp<FTLDestinationComponent>(mapUid);

        if (!_mapLoader.TryLoadGrid(mapId, ent.Comp.Path, out var grid))
            return;

        _mapTiles.Clear();
        var bounds = Comp<MapGridComponent>(grid.Value).LocalAABB;

        _biomes.ReserveTiles(mapUid, bounds, _mapTiles);

        if (!_protoManager.TryIndex(ent.Comp.RoomPool,  out var roomPool))
            return;



        foreach (var room in roomPool.Rooms)
        {
            var coordinates = new Vector2(_random.Next(25, 50), _random.Next(25, 50));

            if (!_mapLoader.TryLoadGrid(mapId, _protoManager.Index(room).Path, out var roomGrid))
                continue;

            _transform.SetCoordinates(roomGrid.Value, new EntityCoordinates(mapUid, coordinates.X, coordinates.Y));
            var roomBounds = Comp<MapGridComponent>(roomGrid.Value).LocalAABB;
            _biomes.ReserveTiles(mapUid, roomBounds, _mapTiles);
        }


        _map.InitializeMap(mapUid);

    }
}
