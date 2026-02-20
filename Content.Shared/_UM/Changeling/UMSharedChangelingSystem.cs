using Content.Shared._UM.Changeling.Sting.Components;
using Content.Shared.Body;
using Content.Shared.Changeling.Systems;
using Content.Shared.Cloning;
using Content.Shared.IdentityManagement;
using Content.Shared.Stunnable;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._UM.Changeling;

/// <summary>
/// This handles...
/// </summary>
public sealed class UMSharedChangelingSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedVisualBodySystem _visualBody = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly SharedCloningSystem _cloning = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly IdentitySystem _identity = default!;
    [Dependency] private readonly SharedChangelingIdentitySystem _changelingIdentity = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
    }

    /// <summary>
    /// Transforms <param name="target"/> into <param name="ent"/>
    /// </summary>
    public void TransformInto(EntityUid target, EntityUid ent)
    {
        var comp = EnsureComp<ChangedGenomeComponent>(target);

        if (!_prototype.Resolve(comp.Settings, out var settings))
            return;

        var cloneEnt = _changelingIdentity.CloneToPausedMap(settings, ent);
        var ownerClone = _changelingIdentity.CloneToPausedMap(settings, target);

        comp.OriginalEntity = ownerClone;
        comp.TransformedEntity = cloneEnt;

        if (!Exists(comp.TransformedEntity) || _net.IsClient)
            return;

        Transform((target, comp), comp.TransformedEntity);

        comp.EndTime = _timing.CurTime + comp.Duration;
    }

    public void Transform(Entity<ChangedGenomeComponent?> ent, EntityUid? cloneEnt)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        if (!Exists(cloneEnt) || _net.IsClient)
            return;

        _visualBody.CopyAppearanceFrom(cloneEnt.Value, ent.Owner);
        _cloning.CloneComponents(cloneEnt.Value, ent.Owner, ent.Comp.Settings);
        _metaData.SetEntityName(ent.Owner, Name(cloneEnt.Value), raiseEvents: false);
        _identity.QueueIdentityUpdate(ent.Owner);
    }
}
