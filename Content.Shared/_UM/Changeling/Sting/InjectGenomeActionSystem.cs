using Content.Shared._UM.Changeling.Sting.Components;
using Content.Shared.Body;
using Content.Shared.Changeling.Components;
using Content.Shared.Changeling.Systems;
using Content.Shared.Cloning;
using Content.Shared.Forensics.Systems;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Robust.Shared.Random;

namespace Content.Shared._UM.Changeling.Sting;

/// <summary>
/// This handles...
/// </summary>
public sealed class InjectGenomeActionSystem : EntitySystem
{
    [Dependency] private readonly ChangelingClonerSystem _changelingClonerSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedForensicsSystem _forensics = default!;
    [Dependency] private readonly SharedVisualBodySystem _visualBody = default!;
    [Dependency] private readonly SharedCloningSystem _cloning = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly IdentitySystem _identity = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

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

        if (!HasComp<HumanoidProfileComponent>(args.Target))
            return;

        if (!TryComp<ChangelingIdentityComponent>(args.Performer, out var changelingIdentity))
            return;

        if (changelingIdentity.ConsumedIdentities.Count < 2) // Return if we haven't absorbed anyone
            return;

        var cloneEnt = _random.Pick(changelingIdentity.ConsumedIdentities);

        if (!Exists(cloneEnt))
            return;

        _visualBody.CopyAppearanceFrom(cloneEnt, args.Target);
        _cloning.CloneComponents(cloneEnt, args.Target, ent.Comp.Settings);
        _metaData.SetEntityName(args.Target, Name(cloneEnt), raiseEvents: false);
        _identity.QueueIdentityUpdate(args.Target);

        args.Handled = true;

        _popup.PopupClient(Loc.GetString(ent.Comp.UserPopup, ("target", args.Target)), args.Performer, args.Performer);

        if (!ent.Comp.Silent)
            _popup.PopupEntity(Loc.GetString(ent.Comp.TargetPopup), args.Target, args.Target);
    }
}
