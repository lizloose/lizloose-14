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
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
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

        Dirty(ent);
    }

    private Entity<EnergyComponent> SpawnEnergy(ContainerSlot container, Energy energytype)
    {
        var coords = new EntityCoordinates(container.Owner, Vector2.Zero);
        var uid = _entityManager.CreateEntityUninitialized(null, coords, null);

        var energy = new EnergyComponent() { Amount = energytype.Amount, Max = energytype.Max, UpdateAmount = energytype.UpdateAmount, Alert = energytype.Alert, ContainerOwner = container.Owner};
        AddComp(uid, energy);

        _metadata.SetEntityName(uid, $"energy - {energytype.Name}", raiseEvents: false);
        _container.Insert(uid, container, force: true);

        return (uid, energy);
    }

}
