using Content.Shared._UM.Changeling.Sting.Components;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Body;
using Content.Shared.Changeling.Components;
using Content.Shared.Changeling.Systems;
using Content.Shared.Cloning;
using Content.Shared.Forensics.Components;
using Content.Shared.Forensics.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Popups;

namespace Content.Shared._UM.Changeling.Sting;

/// <summary>
/// This handles...
/// </summary>
public sealed class ExtractGenomeActionSystem : EntitySystem
{
    [Dependency] private readonly SharedChangelingIdentitySystem _changelingIdentitySystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedForensicsSystem _forensics = default!;

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

        if (!TryComp<ChangelingIdentityComponent>(args.Performer, out var identityStorage))
            return;

        foreach (var identity in identityStorage.ConsumedIdentities)
        {
            var name = Name(identity);
            var targetName = Name(args.Target);

            if (!TryComp<DnaComponent>(identity, out var identityDna) || !TryComp<DnaComponent>(args.Target, out var targetDna))
                return;

            if (identityDna.DNA == targetDna.DNA)
            {
                _popup.PopupClient(Loc.GetString("changeling-extract-genome-sting-already-absorbed", ("target", args.Target)), args.Performer, args.Performer);
                return;
            }
        }

        if (HasComp<RottingComponent>(args.Target))
        {
            _popup.PopupClient(Loc.GetString($"{"changeling-devour-attempt-failed-rotting"}"), args.Performer, args.Performer, PopupType.Medium);
            return;
        }

        if (!TryComp<BodyComponent>(args.Target, out var body) && !HasComp<HumanoidProfileComponent>(args.Target))
            return;

        _changelingIdentitySystem.CloneToPausedMap((args.Performer, identityStorage), args.Target);
        args.Handled = true;

        _popup.PopupClient(Loc.GetString(ent.Comp.UserPopup, ("target", args.Target)), args.Performer, args.Performer);

        if (!ent.Comp.Silent)
            _popup.PopupEntity(Loc.GetString(ent.Comp.TargetPopup), args.Target, args.Target);

    }
}
