using System.Numerics;
using Content.Shared.Planets.Prototypes;
using Robust.Shared.EntitySerialization;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;

namespace Content.Server.Planets;


public sealed partial class PlanetSystem
{
    private readonly List<(Vector2i, Tile)> _tiles = new();

    private void LoadGrid(EntityUid gridUid,
        MapGridComponent grid,
        Vector2i origin,
        Angle rotation,
        ResPath resPath,
        HashSet<Vector2i>? reservedTiles = null,
        bool clearExisting = true)
    {

        Log.Debug(origin.ToString());

        _map.CreateMap(out var mapId, false);

        var opts = new MapLoadOptions
        {
            MergeMap = mapId,
            DeserializationOptions = DeserializationOptions.Default with {PauseMaps = true},
            ExpectedCategory = FileCategory.Grid
        };

        if (!_mapLoader.TryLoadGeneric(resPath, out var res, opts))
            return;

        Log.Debug("map count: " + res.Maps.Count.ToString());

        var templateMapUid = _map.GetMapOrInvalid(mapId);

        Log.Debug(templateMapUid.ToString());

        Log.Debug("grid Count: " + res.Grids.Count.ToString());

        if (!res.Grids.TryFirstOrNull(out var templateGrid))
        {
            Log.Debug("Still no grid wtf");
            return;
        }

        var dimensions = new Vector2i(20, 20);

        var roomCenter = (origin + dimensions / 2f) * grid.TileSize;
        var tileOffset = -roomCenter + grid.TileSizeHalfVector;
        _tiles.Clear();


        var tiles = _map.GetAllTiles(templateMapUid, templateGrid, true);

        foreach (var tile in tiles)
        {
            var tilePos = tile.GridIndices + origin;
            var rounded = tilePos;
            _tiles.Add((rounded, tile.Tile));
        }

        var bounds = Comp<MapGridComponent>(templateGrid.Value).LocalAABB;


        Log.Debug(bounds.ToString());
        _map.SetTiles(gridUid, grid, _tiles);

        // Load entities
        // TODO: I don't think engine supports full entity copying so we do this piece of shit.

        foreach (var templateEnt in _lookup.GetEntitiesIntersecting(templateGrid.Value, bounds, LookupFlags.All))
        {
            var templateXform = Transform(templateEnt);

            var childPos = templateXform.LocalPosition + origin;

            if (!clearExisting && reservedTiles?.Contains(childPos.Floored()) == true)
                continue;

            var childRot = templateXform.LocalRotation;
            var protoId = _metaQuery.GetComponent(templateEnt).EntityPrototype?.ID;

            // TODO: Copy the templated entity as is with serv

            var ent = Spawn(protoId, new EntityCoordinates(gridUid, childPos));

            var childXform = Transform(ent);
            var anchored = templateXform.Anchored;
            _transform.SetLocalRotation(ent, childRot, childXform);

            // If the templated entity was anchored then anchor us too.
            if (anchored && !childXform.Anchored)
                _transform.AnchorEntity((ent, childXform), (gridUid, grid));
            else if (!anchored && childXform.Anchored)
                _transform.Unanchor(ent, childXform);
        }

        _map.DeleteMap(mapId);
    }
}
