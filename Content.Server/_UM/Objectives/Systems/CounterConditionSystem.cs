using Content.Server._UM.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Objectives.Components;

namespace Content.Server._UM.Objectives.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class CounterConditionSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GenericCounterComponent, ObjectiveGetProgressEvent>(OnCounterGetProgress);

    }

    private void OnCounterGetProgress(Entity<GenericCounterComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = GetProgress(ent, _number.GetTarget(ent.Owner));
    }

    private float GetProgress(Entity<GenericCounterComponent> ent, int target)
    {
        // prevent divide-by-zero
        if (target == 0)
            return 1f;

        if (ent.Comp.Count >= target)
            return 1f;

        return (float) ent.Comp.Count / target;
    }
}
