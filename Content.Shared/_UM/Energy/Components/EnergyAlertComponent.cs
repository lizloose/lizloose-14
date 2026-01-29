using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Energy.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, AutoGenerateComponentState, NetworkedComponent]
public sealed partial class EnergyAlertComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public Dictionary<string, ProtoId<AlertPrototype>> Types = new();
}
