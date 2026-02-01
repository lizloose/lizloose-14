using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Energy.Components;

/// <summary>
/// This is used for managing different types of energies.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class EnergyContainerComponent : Component
{
    [ViewVariables]
    public Dictionary<string, Entity<EnergyComponent>> EnergyTypes = new();

    /// <summary>
    /// Energy types are specified here. This is only used on map init.
    /// </summary>
    [DataField(required:true)]
    public List<EntProtoId> Types;
}
