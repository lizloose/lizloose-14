using Content.Shared._UM.Energy;
using Content.Shared._UM.Energy.Components;
using Content.Shared.Alert.Components;

namespace Content.Client._UM.Energy;

/// <summary>
/// This handles...
/// </summary>
public sealed class EnergyAlertSystem : EntitySystem
{
    [Dependency] private readonly SharedEnergySystem _energySystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EnergyAlertComponent, GetGenericAlertCounterAmountEvent>(OnGetCounterAmount);
    }

    private void OnGetCounterAmount(Entity<EnergyAlertComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Handled)
            return;

        foreach (var type in ent.Comp.Types)
        {
            if (type.Value == args.Alert  && _energySystem.TryGetEnergy(ent.Owner, type.Key, out var energy))
            {
                args.Amount = energy.Energy.Int();
            }
        }
    }

}
