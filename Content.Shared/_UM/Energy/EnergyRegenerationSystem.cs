using Content.Shared._UM.Energy.Components;
using Robust.Shared.Timing;

namespace Content.Shared._UM.Energy;

/// <summary>
/// This handles...
/// </summary>
public sealed class EnergyRegenerationSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedEnergySystem _energySystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EnergyRegenerationComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<EnergyComponent, EnergyRegenerationComponent>();

        while (query.MoveNext(out var uid, out var energyComp, out var regenComp))
        {
            if (regenComp.NextUpdate > curTime)
                continue;

            foreach (var regenEnergy in regenComp.Types)
            {
                if (!_energySystem.TryGetEnergy(uid, regenEnergy.Key, out var energy))
                    continue;

                if (energy.Energy > regenEnergy.Value.MaxRegenAmount)
                    continue;

                if (!_energySystem.TryAddEnergy(uid, regenEnergy.Key, regenEnergy.Value.Amount))
                    continue;
            }
            regenComp.NextUpdate += regenComp.UpdateInterval;
            Dirty(uid, regenComp);
        }
    }

    private void OnMapInit(Entity<EnergyRegenerationComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.UpdateInterval;
        Dirty(ent);
    }
}
