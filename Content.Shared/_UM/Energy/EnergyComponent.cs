using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._UM.Energy;

/// <summary>
/// This is used for keeping track of a single type of energy
/// Do not directly give this to entities!
/// Use EntityContainer and EntityContainerSystem!
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class EnergyComponent : Component
{
    /// <summary>
    /// Amount of energy we should start with
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Amount { get; set; }

    /// <summary>
    /// How much energy we passively regenerate every UpdateInterval
    /// </summary>
    [DataField, AutoNetworkedField]
    public int PassiveRegen { get; set; }

    /// <summary>
    /// Update interval for passive regeneration
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(2);

    /// <summary>
    /// The maximum amount we can regenerate (separate from Max)
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MaxRegen { get; set; }

    /// <summary>
    /// Actual maximum amount of energy we can have.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Max = 999;

    /// <summary>
    /// Prototype for Status alert.
    /// So far only works with GenericCounterAlert
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<AlertPrototype>? Alert;

    [DataField, AutoNetworkedField]
    public EntityUid ContainerOwner;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;
}
