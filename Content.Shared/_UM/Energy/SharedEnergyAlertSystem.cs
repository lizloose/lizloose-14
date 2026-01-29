using Content.Shared._UM.Energy.Components;
using Content.Shared.Alert;
using Content.Shared.Alert.Components;
using Robust.Shared.Network;

namespace Content.Shared._UM.Energy;

/// <summary>
/// This handles...
/// </summary>
public sealed class SharedEnergyAlertSystem : EntitySystem
{
    [Dependency] private readonly SharedEnergySystem _energySystem = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EnergyAlertComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<EnergyAlertComponent> ent, ref MapInitEvent args)
    {
        foreach (var energyType in ent.Comp.Types)
        {
            if (!_energySystem.HasEnergyType(ent.Owner, energyType.Key))
                continue;

            _alerts.ShowAlert(ent.Owner, energyType.Value);
        }
    }
}
