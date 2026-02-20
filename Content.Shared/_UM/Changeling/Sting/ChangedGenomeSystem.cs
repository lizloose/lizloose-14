using Content.Shared._UM.Changeling.Sting.Components;
using Content.Shared.Stunnable;
using Robust.Shared.Timing;

namespace Content.Shared._UM.Changeling.Sting;

/// <summary>
/// This is shitcode
/// </summary>
public sealed class ChangedGenomeSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly UMSharedChangelingSystem _changeling = default!;

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
                _changeling.Transform(uid, comp.OriginalEntity.Value, comp.Settings);

            _stunSystem.TryUpdateParalyzeDuration(uid, TimeSpan.FromSeconds(8));
            RemComp<ChangedGenomeComponent>(uid);
        }
    }
}
