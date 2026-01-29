using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._UM.Energy.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, AutoGenerateComponentState, NetworkedComponent]
public sealed partial class EnergyComponent : Component
{
    [DataField("types"), AutoNetworkedField]
    public Dictionary<string, BaseEnergy> Types = new();
}

[Serializable, NetSerializable]
[DataDefinition]
public sealed partial class BaseEnergy
{
    /// <summary>
    /// How much energy is there
    /// </summary>
    [DataField]
    public FixedPoint2 Energy;

    /// <summary>
    /// Maximum amount of energy we can have
    /// </summary>
    [DataField]
    public FixedPoint2 MaxEnergy = 1000;
}
