using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared._UM.Energy.Components;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Energy;

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

        foreach (var type in ent.Comp.Types)
        {
            CreateEnergy(ent, type);
        }
    }

    public void CreateEnergy(Entity<EnergyContainerComponent> ent, EntProtoId protoId)
    {
        var containerManager = EnsureComp<ContainerManagerComponent>(ent);
        var container = _container.EnsureContainer<Container>(ent, "energy", containerManager);

        var energy = SpawnEnergy(container, protoId);
        _entityManager.InitializeAndStartEntity(energy.Owner);
        _container.Insert(energy.Owner, container, force: true);
        Dirty(energy);
        Dirty(ent.Owner, containerManager);
    }

    private Entity<EnergyComponent> SpawnEnergy(Container container, EntProtoId protoId)
    {
        var coords = new EntityCoordinates(container.Owner, Vector2.Zero);
        var uid = _entityManager.CreateEntityUninitialized(protoId, coords);
        var energy = EnsureComp<EnergyComponent>(uid);
        _metadata.SetEntityName(uid, $"energy - {energy.Name}", raiseEvents: false);
        return (uid, energy);
    }

    /// <summary>
    /// If true, returns an EnergyComponent if a container with name exists
    /// </summary>
    public bool TryGetEnergy(Entity<EnergyContainerComponent?> ent, string name, [NotNullWhen(true)] out Entity<EnergyComponent>? energy)
    {
        energy = null;

        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (!ent.Comp.EnergyTypes.TryGetValue(name, out var energyEnt))
            return false;
        energy = energyEnt;
        return true;
    }

    /// <summary>
    /// If true, returns the current amount an energy type has if a container with its name exists
    /// </summary>
    public bool TryGetEnergyAmount(Entity<EnergyContainerComponent?> ent, string name, [NotNullWhen(true)] out int? amount)
    {
        amount = null;

        if (!TryGetEnergy(ent, name, out var energy))
            return false;

        amount = energy.Value.Comp.Amount;
        return true;
    }

    /// <summary>
    /// If true, adds "amount" to an energy type has if a container with its name exists
    /// </summary>
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

    /// <summary>
    /// If true, deducts "amount" from an energy type has if a container with its name exists
    /// </summary>
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

    /// <summary>
    /// returns true if specified energy type can spend "amount"
    /// </summary>
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

    /// <summary>
    /// returns true if energycomponent can spend "amount"
    /// </summary>
    public bool CanSpendEnergy(Entity<EnergyComponent?> ent, int amount)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (ent.Comp.Amount < amount)
            return false;

        return true;
    }
}
