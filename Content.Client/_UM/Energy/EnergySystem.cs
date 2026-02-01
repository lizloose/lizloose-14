using Content.Shared._UM.Energy;
using Content.Shared._UM.Energy.Components;
using Content.Shared.Alert.Components;

namespace Content.Client._UM.Energy;

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
            if (!_energyContainer.TryGetEnergy((ent, ent.Comp), type.Key, out var energy))
                continue;

            if (args.Alert == energy.Value.Comp.Alert)
                args.Amount = energy.Value.Comp.Amount;
        }
    }
}
