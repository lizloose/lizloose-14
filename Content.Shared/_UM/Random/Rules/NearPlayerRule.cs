using Content.Shared.Mind.Components;
using Content.Shared.Random.Rules;
using Robust.Shared.Map;

namespace Content.Shared._UM.Random.Rules;

public sealed partial class PlayersNearbyRule : RulesRule
{
    [DataField]
    public float Range = 5f;

    [DataField]
    public int Count = 3;

    [DataField]
    public MindState State = MindState.None;


    public override bool Check(EntityManager entManager, EntityUid uid)
    {
        if (!entManager.TryGetComponent(uid, out TransformComponent? xform))
            return false;

        var transform = entManager.System<SharedTransformSystem>();
        var entityLookup = entManager.System<EntityLookupSystem>();


        var entities = entityLookup.GetEntitiesInRange<MindExaminableComponent>(xform.Coordinates, Range, LookupFlags.Uncontained);
        var playerCount = 0;

        foreach (var ent in entities)
        {
            if (ent.Owner == uid)
                continue;

            if (ent.Comp.State != State)
                continue;
            playerCount++;
        }

        if (playerCount >= Count)
            return !Inverted;

        return Inverted;
    }
}
