using Content.Shared._UM.Energy;
using Content.Shared.Alert.Components;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Client._UM.Energy;

/// <summary>
/// This handles...
/// </summary>
public sealed class EnergySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EnergyContainerComponent, GetGenericAlertCounterAmountEvent>(OnGetGenericAlertCounterAmount);
    }

    private void OnGetGenericAlertCounterAmount(Entity<EnergyContainerComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        foreach (var type in ent.Comp.EnergyTypes)
        {
            if (!_container.TryGetContainer(ent, $"energytype@{type}", out var container))
                continue;

            if (container is ContainerSlot slot && slot.ContainedEntity != null)
            {
                if (!TryComp<EnergyComponent>(slot.ContainedEntity.Value, out var energy))
                    continue;

                if (args.Alert == energy.Alert)
                    args.Amount = energy.Amount;
            }
        }
    }
}
