using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._UM.Energy;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class EnergyContainerComponent : Component
{
    [AutoNetworkedField]
    public HashSet<string> EnergyTypes = new(1);

    [DataField(required:true)]
    public Dictionary<string, EntProtoId> Types;
}
