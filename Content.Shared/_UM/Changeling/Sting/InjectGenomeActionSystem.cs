using Content.Shared._UM.Changeling.Sting.Components;
using Content.Shared.Changeling.Components;
using Content.Shared.Humanoid;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Random;

namespace Content.Shared._UM.Changeling.Sting;

/// <summary>
/// This handles...
/// </summary>
public sealed class InjectGenomeActionSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly UMSharedChangelingSystem _changeling = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InjectGenomeActionComponent, InjectGenomeActionEvent>(OnGenomeInjectAction);
    }

    private void OnGenomeInjectAction(Entity<InjectGenomeActionComponent> ent, ref InjectGenomeActionEvent args)
    {
        if (args.Handled)
            return;

        if (args.Target == args.Performer)
            return;

        if (_net.IsClient) //I know, but sigh.
            return;

        if (!HasComp<HumanoidProfileComponent>(args.Target))
            return;

        if (HasComp<ChangedGenomeComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString(ent.Comp.AlreadyChangedPopup, ("target", args.Target)), args.Performer, args.Performer);
            return;
        }

        if (!TryComp<ChangelingIdentityComponent>(args.Performer, out var changelingIdentity))
            return;

        if (changelingIdentity.ConsumedIdentities.Count < 2) // Return if we haven't absorbed anyone
            return;

        var cloneEnt = _random.Pick(changelingIdentity.ConsumedIdentities);

        if (!Exists(cloneEnt) || _net.IsClient)
            return;

        _changeling.TransformInto(args.Target, cloneEnt, ent.Comp.Duration);

        _popup.PopupEntity(Loc.GetString(ent.Comp.UserPopup, ("target", args.Target)), args.Performer, args.Performer);

        if (!ent.Comp.Silent)
            _popup.PopupEntity(Loc.GetString(ent.Comp.TargetPopup), args.Target, args.Target);

        args.Handled = true;
    }
}
