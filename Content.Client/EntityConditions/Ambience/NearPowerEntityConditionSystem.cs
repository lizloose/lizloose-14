using Content.Client.Power.Components;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;

namespace Content.Client.EntityConditions.Ambience;

public sealed partial class NearPowerEntityConditionSystem : EntityConditionSystem<TransformComponent, NearPowerCondition>
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    protected override void Condition(Entity<TransformComponent> entity, ref EntityConditionEvent<NearPowerCondition> args)
    {
        foreach (var uid in _lookup.GetEntitiesInRange(entity.Comp.Coordinates, args.Condition.Range, LookupFlags.Static))
        {
            if (uid == entity.Owner)
                continue;

            if (!TryComp<ApcPowerReceiverComponent>(uid, out var powerComp))
                continue;

            if (powerComp.Powered)
            {
                args.Result = true;
                return;
            }
        }
    }
}


public sealed partial class NearPowerCondition : EntityConditionBase<NearPowerCondition>
{
    [DataField]
    public float Range = 10;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return String.Empty;
    }
}
