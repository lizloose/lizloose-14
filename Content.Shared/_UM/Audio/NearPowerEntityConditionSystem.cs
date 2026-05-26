using Content.Shared.EntityConditions;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Audio;

public sealed partial class NearPowerEntityConditionSystem : EntityConditionSystem<TransformComponent, NearPowerCondition>
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;


    protected override void Condition(Entity<TransformComponent> entity, ref EntityConditionEvent<NearPowerCondition> args)
    {
        foreach (var uid in _lookup.GetEntitiesInRange(entity.Comp.Coordinates, args.Condition.Range, LookupFlags.Static))
        {
            if (uid == entity.Owner)
                continue;

            SharedApcPowerReceiverComponent? comp = null;

            if (!_power.ResolveApc(uid, ref comp))
                continue;

            if (comp.Powered)
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
