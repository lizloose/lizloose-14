using Robust.Shared.Prototypes;

namespace Content.Shared.Planets.Prototypes;

/// <summary>
/// This is a prototype for...
/// </summary>
[Prototype("planetSpawnRoomPool")]
public sealed partial class PlanetSpawnRoomPoolPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField]
    public float Distance = 30;

    /// <summary>
    /// List of all rooms to spawn on the planet
    /// </summary>
    [DataField]
    public List<ProtoId<PlanetSpawnRoomPrototype>> Rooms = new();
}
