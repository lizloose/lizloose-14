using Robust.Shared.GameStates;

namespace Content.Shared._UM.Energy;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class EnergyActionRequirementComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public string Name;

    [DataField(required: true), AutoNetworkedField]
    public int Amount;
}
