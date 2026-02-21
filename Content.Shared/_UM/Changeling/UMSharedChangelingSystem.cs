using Content.Shared._UM.Changeling.Components;
using Content.Shared._UM.Changeling.Sting.Components;
using Content.Shared.Body;
using Content.Shared.Changeling.Components;
using Content.Shared.Changeling.Systems;
using Content.Shared.Cloning;
using Content.Shared.FixedPoint;
using Content.Shared.Forensics.Components;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._UM.Changeling;

/// <summary>
/// This handles...
/// </summary>
public abstract class UMSharedChangelingSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedVisualBodySystem _visualBody = default!;
    [Dependency] private readonly SharedCloningSystem _cloning = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly IdentitySystem _identity = default!;
    [Dependency] private readonly SharedChangelingIdentitySystem _changelingIdentity = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    /// <summary>
    /// Transforms <param name="target"/> into <param name="ent"/>
    /// Used for transformation sting/inject genome sting
    /// they will revert after a certain duration
    /// </summary>
    public void TransformInto(EntityUid target, EntityUid ent, TimeSpan duration)
    {
        var comp = EnsureComp<ChangedGenomeComponent>(target);
        comp.Duration = duration;

        if (!_prototype.Resolve(comp.Settings, out var settings))
            return;

        var cloneEnt = _changelingIdentity.CloneToPausedMap(settings, ent);
        var ownerClone = _changelingIdentity.CloneToPausedMap(settings, target);

        comp.OriginalEntity = ownerClone;
        comp.TransformedEntity = cloneEnt;

        if (!Exists(comp.TransformedEntity) || _net.IsClient)
            return;

        Transform(target, comp.TransformedEntity.Value, comp.Settings);

        comp.EndTime = _timing.CurTime + comp.Duration;
    }

    /// <summary>
    /// Transforms <paramref name="ent"/> into <paramref name="cloneEnt"/>
    /// </summary>
    /// <param name="settings">The cloning settings to use</param>
    /// <param name="ent">The entity to transform</param>
    /// <param name="cloneEnt">The entity to clone from</param>
    ///
    public void Transform(EntityUid ent, EntityUid cloneEnt, ProtoId<CloningSettingsPrototype> settings)
    {
        if (!Exists(cloneEnt) || _net.IsClient)
            return;

        _visualBody.CopyAppearanceFrom(cloneEnt, ent);
        _cloning.CloneComponents(cloneEnt, ent, settings);
        _metaData.SetEntityName(ent, Name(cloneEnt), raiseEvents: false);
        _identity.QueueIdentityUpdate(ent);
    }

    /// <summary>
    /// Checks if <paramref name="target"/> can extract the Dna from <paramref name="ent"/><br/>
    /// Returns false if DNA is already absorbed
    /// </summary>
    public bool CanExtractDna(Entity<ChangelingIdentityComponent?> ent, EntityUid target)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (!HasComp<BodyComponent>(target) && !HasComp<HumanoidProfileComponent>(target))
            return false;

        foreach (var identity in ent.Comp.ConsumedIdentities)
        {
            if (!TryComp<DnaComponent>(identity, out var identityDna) || !TryComp<DnaComponent>(target, out var targetDna))
                return false;

            if (identityDna.DNA == targetDna.DNA)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Tries to extract the DNA from <paramref name="target"/> and stores it on <paramref name="ent"/><br/>
    /// Returns false if Dna is already absorbed
    /// </summary>
    public bool TryExtractDna(Entity<ChangelingIdentityComponent?> ent, EntityUid target)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (!CanExtractDna((ent, ent.Comp), target))
            return false;

        _changelingIdentity.CloneToPausedMap((ent, ent.Comp), target);

        return true;
    }

    public virtual bool TryAddStorePoints(EntityUid ent, FixedPoint2 points)
    {
        return false;
    }
}
