using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Spiders.Builder;

/// <summary>
/// This is a prototype for things that can be built with the "Hive Builder" ability. Used by spiders
/// </summary>
[Prototype]
public sealed partial class HiveBuildPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The name associated with the prototype
    /// </summary>
    [DataField("name"), ViewVariables(VVAccess.ReadOnly)]
    public string SetName { get; private set; } = "Unknown";

    /// <summary>
    /// The prototype to build
    /// </summary>
    [DataField(required:true), ViewVariables(VVAccess.ReadOnly)]
    public EntProtoId Prototype { get; private set; }

    /// <summary>
    /// Number of points consumed when built
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public int Cost { get; private set; } = 1;

    /// <summary>
    /// The time it takes to build
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public float Delay { get; private set; } = 1f;
}
