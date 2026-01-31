using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Energy;

/// <summary>
/// This is used for managing different types of energies.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class EnergyContainerComponent : Component
{

    [AutoNetworkedField]
    public HashSet<string> EnergyTypes = new(1);

    /// <summary>
    /// Energy types are specified here. This is cleared after mapinit.
    /// </summary>
    [DataField(required:true)]
    public Dictionary<string, EntProtoId> Types;
}
