using System.Diagnostics.CodeAnalysis;
using Content.Shared.Alert;
using Robust.Shared.Timing;

namespace Content.Shared._UM.Energy;

/// <summary>
/// This handles...
/// </summary>
public sealed class SharedEnergySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EnergyComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<EnergyComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (curTime < comp.NextUpdate)
                continue;

            comp.NextUpdate += comp.UpdateInterval;
            Dirty(uid, comp);

            foreach (var energyType in comp.Types)
            {
                TryAddEnergy(uid, energyType.Key, energyType.Value.UpdateAmount);
            }
        }
    }

    private void OnMapInit(Entity<EnergyComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.UpdateInterval;

        foreach (var type in ent.Comp.Types)
        {
            if (type.Value.Alert != null)
            {
                _alerts.ShowAlert(ent.Owner, type.Value.Alert.Value);
            }
        }
    }

    public bool HasEnergyType(Entity<EnergyComponent?> ent, string type)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        return ent.Comp.Types.ContainsKey(type);
    }

    public bool TryAddEnergy(Entity<EnergyComponent?> ent, string type, int amount)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (!ent.Comp.Types.TryGetValue(type, out var energy))
            return false;

        energy.Amount = Math.Min(energy.Max, energy.Amount + amount);
        ent.Comp.Types[type] = energy;

        Dirty(ent);
        return true;
    }

    public bool CanSpendEnergy(Entity<EnergyComponent?> ent, string type, int amount)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (!ent.Comp.Types.TryGetValue(type, out var energy))
            return false;

        return energy.Amount >= amount;
    }

    public bool TrySpendEnergy(Entity<EnergyComponent?> ent, string type, int amount)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (!ent.Comp.Types.TryGetValue(type, out var energy))
            return false;

        if (!CanSpendEnergy((ent, ent.Comp), type, amount))
            return false;

        energy.Amount -= amount;
        ent.Comp.Types[type] = energy;

        Dirty(ent);
        return ent.Comp.Types[type].Amount >= amount;
    }
}
