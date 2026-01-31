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

    [DataField]
    public List<Energy> Types;
}


[DataDefinition]
[Serializable, NetSerializable]
public partial struct Energy
{
    [DataField]
    public string Name;

    [DataField]
    public int Amount;

    [DataField]
    public int PassiveRegen;

    [DataField]
    public int Max = 999;

    [DataField]
    public int MaxRegen = 999;

    [DataField]
    public ProtoId<AlertPrototype>? Alert;
}
