using Robust.Shared.Prototypes;

namespace Content.Shared.Planets.Prototypes;

/// <summary>
/// This is a prototype for...
/// </summary>
[Prototype()]
public sealed partial class PlanetSpawnGridPoolPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField]
    public int Distance = 150;

    /// <summary>
    /// List of all rooms to spawn on the planet
    /// </summary>
    [DataField]
    public List<ProtoId<PlanetSpawnGridPrototype>> Grids = new();
}
