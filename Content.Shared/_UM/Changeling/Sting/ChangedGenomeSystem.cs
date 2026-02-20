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
    [Dependency] private readonly UMSharedChangelingSystem _changeling = default!;


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

            if (comp.OriginalEntity != null)
                _changeling.Transform(uid, comp.OriginalEntity.Value);

            _stunSystem.TryUpdateParalyzeDuration(uid, TimeSpan.FromSeconds(4));
            RemComp<ChangedGenomeComponent>(uid);
        }
    }

    private void OnMapInit(Entity<ChangedGenomeComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.EndTime = _timing.CurTime + ent.Comp.Duration;
    }

}
