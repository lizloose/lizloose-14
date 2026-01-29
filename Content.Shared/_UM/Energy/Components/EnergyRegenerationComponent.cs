using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._UM.Energy.Components;

/// <summary>
/// This is used for auto regenerating energy
/// </summary>
[RegisterComponent, AutoGenerateComponentState, AutoGenerateComponentPause, NetworkedComponent]
public sealed partial class EnergyRegenerationComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<string, EnergyRegen> Types = new();

    /// <summary>
    /// How long it takes to regenerate once.
    /// </summary>
    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

    /// <summary>
    /// The time when the next regeneration will occur.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate;
}

[Serializable, NetSerializable]
[DataDefinition]
public sealed partial class EnergyRegen
{
    /// <summary>
    /// The amount of to add.
    /// </summary>
    [DataField]
    public FixedPoint2 Amount = 1;

    /// <summary>
    /// Maximum amount to regenerate to.
    /// </summary>
    [DataField]
    public FixedPoint2? MaxRegenAmount;
}
