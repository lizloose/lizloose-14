using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared.Alert;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._UM.Energy;

/// <summary>
/// This handles...
/// </summary>
public sealed class EnergyContainerSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly MetaDataSystem _metadata = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EnergyContainerComponent, MapInitEvent>(OnContainerMapInit);
    }

    private void OnContainerMapInit(Entity<EnergyContainerComponent> ent, ref MapInitEvent args)
    {
        if (_net.IsClient)
            return;

        var containerManager = EnsureComp<ContainerManagerComponent>(ent);

        foreach (var type in ent.Comp.Types)
        {
            var container = _container.EnsureContainer<ContainerSlot>(ent, $"energytype@{type.Name}", containerManager);

            var alert = type.Alert;
            Log.Debug("Alert: " + alert);

            var energy = SpawnEnergy(container, type);
            _entityManager.InitializeAndStartEntity(energy);
            ent.Comp.EnergyTypes.Add(type.Name);
            Dirty(energy);
        }
        ent.Comp.Types.Clear();
        Dirty(ent);
    }

    private Entity<EnergyComponent> SpawnEnergy(ContainerSlot container, Energy energytype)
    {
        var coords = new EntityCoordinates(container.Owner, Vector2.Zero);
        var uid = _entityManager.CreateEntityUninitialized(null, coords);

        var energy = new EnergyComponent() { Amount = energytype.Amount, Max = energytype.Max, MaxRegen = energytype.MaxRegen, PassiveRegen = energytype.PassiveRegen, Alert = energytype.Alert, ContainerOwner = container.Owner};
        AddComp(uid, energy);

        _metadata.SetEntityName(uid, $"energy - {energytype.Name}", raiseEvents: false);
        _container.Insert(uid, container, force: true);

        return (uid, energy);
    }

    public bool TryGetEnergy(Entity<EnergyContainerComponent?> ent, string name, [NotNullWhen(true)] out Entity<EnergyComponent>? energy)
    {
        energy = null;

        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (!_container.TryGetContainer(ent, $"energytype@{name}", out var container))
            return false;

        if (container is ContainerSlot slot && slot.ContainedEntity != null)
        {
            if (!TryComp<EnergyComponent>(slot.ContainedEntity.Value, out var energyComp))
                return false;

            energy = (slot.ContainedEntity.Value, energyComp);
            return true;
        }
        return false;
    }

    public bool TryGetEnergyAmount(Entity<EnergyContainerComponent?> ent, string name, [NotNullWhen(true)] out int? amount)
    {
        amount = null;

        if (!TryGetEnergy(ent, name, out var energy))
            return false;

        amount = energy.Value.Comp.Amount;
        return true;
    }

    public bool TryAddEnergy(Entity<EnergyContainerComponent?> ent, string name, int amount)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (!TryGetEnergy(ent, name, out var energy))
            return false;

        energy.Value.Comp.Amount = Math.Min(energy.Value.Comp.Max, energy.Value.Comp.Amount += amount);
        Dirty(energy.Value);
        return true;
    }

    public bool TrySpendEnergy(Entity<EnergyContainerComponent?> ent, string name, int amount)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (!TryGetEnergy(ent, name, out var energy) || energy.Value.Comp.Amount < amount)
            return false;

        energy.Value.Comp.Amount -= amount;
        Dirty(energy.Value);
        return true;
    }

    public bool CanSpendEnergy(Entity<EnergyContainerComponent?> ent, string name, int amount)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (!TryGetEnergyAmount(ent, name, out var energyAmount))
            return false;

        if (energyAmount < amount)
            return false;

        return true;
    }

    public bool CanSpendEnergy(Entity<EnergyComponent?> ent, int amount)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (ent.Comp.Amount < amount)
            return false;

        return true;
    }
}
