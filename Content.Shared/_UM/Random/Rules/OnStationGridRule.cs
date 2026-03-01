using Content.Shared.Random.Rules;
using Content.Shared.Station.Components;

namespace Content.Shared._UM.Random.Rules;

public sealed partial class OnStationGridRule : RulesRule
{
    public override bool Check(EntityManager entManager, EntityUid uid)
    {
        if (!entManager.TryGetComponent(uid, out TransformComponent? xform))
            return false;

        if (xform.GridUid == null)
        {
            return false;
        }

        if (entManager.HasComponent<StationMemberComponent>(xform.GridUid))
            return !Inverted;

        return Inverted;
    }
}
