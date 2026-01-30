using System.Diagnostics.CodeAnalysis;
using Content.Shared._UM.Energy.Components;
using Content.Shared.Alert;
using Content.Shared.FixedPoint;
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
    }

    /// <summary>
    /// Returns true if an energy type with <paramref name="id"/> exists on <paramref name="ent"/>
    /// </summary>
    public bool HasEnergyType(Entity<EnergyComponent?> ent, string id)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        return ent.Comp.Types.ContainsKey(id);
    }

    /// <summary>
    /// If true, returns the energy type with name <paramref name="id"/> on <paramref name="ent"/>
    /// </summary>
    public bool TryGetEnergy(Entity<EnergyComponent?> ent, string id, [NotNullWhen(true)] out BaseEnergy? energy)
    {
        energy = null;
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        return ent.Comp.Types.TryGetValue(id, out energy);
    }

    /// <summary>
    /// Returns true if an energy type with <paramref name="id"/> has more points than <paramref name="amount"/>
    /// </summary>
    public bool CanSpendEnergy(Entity<EnergyComponent?> ent, string id, FixedPoint2 amount)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (!HasEnergyType(ent, id))
            return false;

        return ent.Comp.Types[id].Energy.Value > amount;
    }

    public bool CanSpendEnergy(BaseEnergy energy, FixedPoint2 amount)
    {
        return energy.Energy > amount;
    }

    public bool TrySpendEnergy(BaseEnergy energy, FixedPoint2 amount)
    {
        if (energy.Energy < amount)
            return false;

        energy.Energy -= amount;
        return true;
    }

    public bool TrySpendEnergy(Entity<EnergyComponent?> ent, string id, FixedPoint2 amount)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (!_timing.IsFirstTimePredicted)
            return true;

        if (!HasEnergyType(ent, id))
            return false;

        if (!CanSpendEnergy(ent.Owner, id, amount))
            return false;

        ent.Comp.Types[id].Energy -= amount;
        Dirty(ent);
        return true;
    }

    public bool TryAddEnergy(Entity<EnergyComponent?> ent, string id, FixedPoint2 amount)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (!TryGetEnergy(ent, id, out var energy))
            return false;

        TryAddEnergy(energy, amount);
        Dirty(ent);
        return true;
    }


    public bool TryAddEnergy(BaseEnergy energy, FixedPoint2 amount)
    {
        if (energy.Energy + amount > energy.MaxEnergy)
            return false;

        energy.Energy += amount;
        return true;
    }
}
