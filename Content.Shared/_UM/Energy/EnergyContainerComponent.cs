using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Energy;

/// <summary>
/// This is used for managing different types of energies.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class EnergyContainerComponent : Component
{
    [ViewVariables]
    public Dictionary<string, Entity<EnergyComponent>> EnergyTypes = new();

    /// <summary>
    /// Energy types are specified here. This is only used on mapinit.
    /// </summary>
    [DataField(required:true)]
    public List<EntProtoId> Types;
}
