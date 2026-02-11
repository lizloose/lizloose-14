using Content.Shared._UM.Changeling.Sting.Components;
using Content.Shared.Body;
using Content.Shared.Changeling.Systems;
using Content.Shared.Cloning;
using Content.Shared.IdentityManagement;
using Content.Shared.Stunnable;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._UM.Changeling.Sting;

/// <summary>
/// This is shitcode
/// </summary>
public sealed class ChangedGenomeSystem : EntitySystem
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

    public override void Update(float frametime)
    {
        base.Update(frametime);
        var curTime = _timing.CurTime;


        var query = EntityQueryEnumerator<ChangedGenomeComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.EndTime > curTime)
                continue;

            Transform((uid, comp), comp.OriginalEntity);
            _stunSystem.TryUpdateParalyzeDuration(uid, TimeSpan.FromSeconds(4));
            RemComp<ChangedGenomeComponent>(uid);
        }
    }

    private void OnMapInit(Entity<ChangedGenomeComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.EndTime = _timing.CurTime + ent.Comp.Duration;
    }

    public void TransformInto(EntityUid ent, EntityUid toClone)
    {
        var comp = EnsureComp<ChangedGenomeComponent>(ent);

        if (!_prototype.Resolve(comp.Settings, out var settings))
            return;

        var cloneEnt = _changelingIdentity.CloneToPausedMap(settings, toClone);
        var ownerClone = _changelingIdentity.CloneToPausedMap(settings, ent);

        comp.OriginalEntity = ownerClone;
        comp.TransformedEntity = cloneEnt;

        if (!Exists(comp.TransformedEntity) || _net.IsClient)
            return;

        Transform((ent, comp), comp.TransformedEntity);

        comp.EndTime = _timing.CurTime + comp.Duration;
    }


    private void Transform(Entity<ChangedGenomeComponent> ent, EntityUid? cloneEnt)
    {
        if (!Exists(cloneEnt) || _net.IsClient)
            return;

        _visualBody.CopyAppearanceFrom(cloneEnt.Value, ent.Owner);
        _cloning.CloneComponents(cloneEnt.Value, ent.Owner, ent.Comp.Settings);
        _metaData.SetEntityName(ent.Owner, Name(cloneEnt.Value), raiseEvents: false);
        _identity.QueueIdentityUpdate(ent.Owner);
    }
}
