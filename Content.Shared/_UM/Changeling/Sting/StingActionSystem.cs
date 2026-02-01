using Content.Shared.Body.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;

namespace Content.Shared._UM.Changeling.Sting;

/// <summary>
/// This handles...
/// </summary>
public sealed class StingActionSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StingActionComponent, StingActionEvent>(OnStingAction);
    }

    private void OnStingAction(Entity<StingActionComponent> ent, ref StingActionEvent args)
    {
        Log.Debug("sting");

        if (!HasComp<BloodstreamComponent>(args.Target))
            return;

        Log.Debug("sting2");
        if (!_solution.TryGetInjectableSolution(args.Target, out var solutionComp, out _))
            return;
        Log.Debug("sting3");

        _solution.Inject(args.Target, solutionComp.Value, ent.Comp.Solution);
    }
}
