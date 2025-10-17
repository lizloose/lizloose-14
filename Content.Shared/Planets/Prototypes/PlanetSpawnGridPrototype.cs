using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Planets.Prototypes;

/// <summary>
/// This is a prototype for...
/// </summary>
[Prototype()]
public sealed partial class PlanetSpawnGridPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField(required: true)]
    public ResPath Path;
}
