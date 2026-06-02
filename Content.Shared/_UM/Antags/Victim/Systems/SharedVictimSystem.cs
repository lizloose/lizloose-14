using Content.Shared._UM.Antags.Victim.Components;
using Robust.Shared.Timing;

namespace Content.Shared._UM.Antags.Victim.Systems;

/// <summary>
/// This handles...
/// </summary>
public abstract partial class SharedVictimSystem : EntitySystem
{
    [Dependency] private IGameTiming _gameTiming = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VictimComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<VictimComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.DetonationTime = _gameTiming.CurTime + ent.Comp.BombDuration;
        ent.Comp.NextUpdate =  _gameTiming.CurTime + ent.Comp.UpdateInterval;
        ent.Comp.BombEnabled = true;
    }
}
