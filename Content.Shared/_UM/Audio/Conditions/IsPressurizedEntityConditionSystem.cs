using Content.Shared._UM.Atmos;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Audio.Conditions;

/// <summary>
/// This handles...
/// </summary>
public sealed class IsPressurizedEntityConditionSystem : EntityConditionSystem<UMAtmosPressurizedComponent, IsPressurizedEntityCondition>
{
    protected override void Condition(Entity<UMAtmosPressurizedComponent> entity, ref EntityConditionEvent<IsPressurizedEntityCondition> args)
    {
        args.Result = entity.Comp.Pressurized;
    }
}

public sealed partial class IsPressurizedEntityCondition : EntityConditionBase<IsPressurizedEntityCondition>
{
    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return String.Empty;
    }
}
