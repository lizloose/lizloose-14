using System.Numerics;
using Content.Shared.Random.Rules;
using Content.Shared.Station.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Shared._UM.Random.Rules;

/// <summary>
/// Returns true if they are near a grid that is a station
/// </summary>
public sealed partial class StationInRangeRule : RulesRule
{
    [DataField]
    public float Range = 10f;

    private List<Entity<MapGridComponent>> _grids = [];

    public override bool Check(EntityManager entManager, EntityUid uid)
    {
        if (!entManager.TryGetComponent(uid, out TransformComponent? xform))
        {
            return false;
        }

        if (xform.GridUid != null)
        {
            return !Inverted;
        }

        var transform = entManager.System<SharedTransformSystem>();
        var mapManager = IoCManager.Resolve<IMapManager>();

        var worldPos = transform.GetWorldPosition(xform);
        var gridRange = new Vector2(Range, Range);

        _grids.Clear();
        mapManager.FindGridsIntersecting(xform.MapID, new Box2(worldPos - gridRange, worldPos + gridRange), ref _grids);

        var stations = 0;

        foreach (var grid in _grids)
        {
            if (entManager.HasComponent<StationMemberComponent>(grid.Owner))
                stations++;
        }

        if (stations > 0)
            return !Inverted;

        return Inverted;
    }
}
