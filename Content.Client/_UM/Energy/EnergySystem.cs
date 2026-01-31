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
    [Dependency] private readonly EnergyContainerSystem _energyContainer = default!;
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
            if (!_energyContainer.TryGetEnergy(ent.Owner, type, out var energy))
                continue;

            if (args.Alert == energy.Value.Comp.Alert)
                args.Amount = energy.Value.Comp.Amount;
        }
    }
}
