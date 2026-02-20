using Content.Shared._UM.Changeling.Sting.Components;
using Content.Shared.Body;
using Content.Shared.Changeling.Components;
using Content.Shared.Changeling.Systems;
using Content.Shared.Cloning;
using Content.Shared.Forensics.Systems;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
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
    [Dependency] private readonly SharedVisualBodySystem _visualBody = default!;
    [Dependency] private readonly SharedCloningSystem _cloning = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
    [Dependency] private readonly IdentitySystem _identity = default!;
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

        if (!HasComp<HumanoidProfileComponent>(args.Target))
            return;

        if (!TryComp<ChangelingIdentityComponent>(args.Performer, out var changelingIdentity))
            return;

        if (changelingIdentity.ConsumedIdentities.Count < 2) // Return if we haven't absorbed anyone
            return;

        var cloneEnt = _random.Pick(changelingIdentity.ConsumedIdentities);

        if (!Exists(cloneEnt) || _net.IsClient)
            return;

        _changeling.TransformInto(args.Target, cloneEnt);

        _popup.PopupEntity(Loc.GetString(ent.Comp.UserPopup, ("target", args.Target)), args.Performer, args.Performer);

        if (!ent.Comp.Silent)
            _popup.PopupEntity(Loc.GetString(ent.Comp.TargetPopup), args.Target, args.Target);

        args.Handled = true;
    }
}
