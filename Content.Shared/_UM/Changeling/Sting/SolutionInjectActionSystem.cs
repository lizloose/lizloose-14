using Content.Shared._UM.Changeling.Sting.Components;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Popups;

namespace Content.Shared._UM.Changeling.Sting;

/// <summary>
/// This handles...
/// </summary>
public sealed class SolutionInjectActionSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SolutionInjectActionComponent, SolutionInjectActionEvent>(OnStingAction);
    }

    private void OnStingAction(Entity<SolutionInjectActionComponent> ent, ref SolutionInjectActionEvent args)
    {
        if (args.Handled)
            return;

        if (!HasComp<BloodstreamComponent>(args.Target))
            return;

        if (!_solution.TryGetInjectableSolution(args.Target, out var solutionComp, out _))
            return;

        _solution.Inject(args.Target, solutionComp.Value, ent.Comp.Solution);
        args.Handled = true;

        _popup.PopupClient(Loc.GetString(ent.Comp.UserPopup, ("target", args.Target)), args.Performer, args.Performer);

        if (!ent.Comp.Silent)
            _popup.PopupEntity(Loc.GetString(ent.Comp.TargetPopup), args.Target, args.Target);
    }
}
