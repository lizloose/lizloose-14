using Robust.Shared.GameStates;

namespace Content.Shared._UM.Energy.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class EnergyActionRequirementComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public string EnergyName;

    [DataField(required: true)]
    public string FancyName;

    [DataField(required: true), AutoNetworkedField]
    public int Amount;

    /// <summary>
    /// If true, will check if we can spend the amount but will not deduct any.
    /// </summary>
    [DataField]
    public bool OnlyCheck = false;

    [DataField]
    public LocId CantFireMessage = "action-popup-energy-requirement";
}
