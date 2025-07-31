using Content.Shared.Parallax.Biomes;
using Content.Shared.Planets.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Planets;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class PlanetSpawnComponent : Component
{
    [DataField(required: true)]
    public ResPath Path;

    [DataField(required: true)]
    public ProtoId<BiomeTemplatePrototype> Biome;

    [DataField]
    public ProtoId<PlanetSpawnRoomPoolPrototype> RoomPool;

}
