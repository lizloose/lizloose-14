using Content.Shared._UM.Changeling.Sting.Components;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Body;
using Content.Shared.Humanoid;
using Content.Shared.Popups;

namespace Content.Shared._UM.Changeling.Sting;

/// <summary>
/// This handles...
/// </summary>
public sealed class ExtractGenomeActionSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    // ReSharper disable once InconsistentNaming
    [Dependency] private readonly UMSharedChangelingSystem _changeling = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ExtractGenomeActionComponent, ExtractGenomeActionEvent>(OnExtractGenomeAction);
    }


    private void OnExtractGenomeAction(Entity<ExtractGenomeActionComponent> ent, ref ExtractGenomeActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_changeling.CanExtractDna(args.Performer, args.Target))
        {
            _popup.PopupClient(Loc.GetString("changeling-extract-genome-sting-already-absorbed", ("target", args.Target)), args.Performer, args.Performer);
            return;
        }

        if (HasComp<RottingComponent>(args.Target))
        {
            _popup.PopupClient(Loc.GetString($"{"changeling-devour-attempt-failed-rotting"}"), args.Performer, args.Performer, PopupType.Medium);
            return;
        }

        if (!HasComp<BodyComponent>(args.Target) && !HasComp<HumanoidProfileComponent>(args.Target))
            return;

        if (!_changeling.TryExtractDna(args.Performer, args.Target))
            return;

        args.Handled = true;

        _popup.PopupClient(Loc.GetString(ent.Comp.UserPopup, ("target", args.Target)), args.Performer, args.Performer);

        if (!ent.Comp.Silent)
            _popup.PopupEntity(Loc.GetString(ent.Comp.TargetPopup), args.Target, args.Target);

    }
}
