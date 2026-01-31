using Content.Shared._UM.Energy;
using Content.Shared.Alert.Components;
using Robust.Shared.Timing;

namespace Content.Client._UM.Energy;

/// <summary>
/// This handles...
/// </summary>
public sealed class EnergySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EnergyComponent, GetGenericAlertCounterAmountEvent>(OnGetGenericAlertCounterAmount);
    }


    private void OnGetGenericAlertCounterAmount(Entity<EnergyComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        foreach (var type in ent.Comp.Types)
        {
            if (type.Value.Alert == args.Alert)
                args.Amount = type.Value.Amount;
        }
    }
}
