using Content.Shared.Hands.Components;

namespace Content.Client.Weapons.Ranged.Components;

[RegisterComponent]
public sealed partial class FireModeVisualsComponent : Component
{
    /// <summary>
    /// Layers to add to the sprite of the player that is holding this entity (for changing gun color)
    /// </summary>
    [DataField]
    public Dictionary<HandLocation, List<PrototypeLayerData>> InhandVisuals = new();

    [DataField]
    public Dictionary<HandLocation, List<PrototypeLayerData>> WieldedInhandVisuals = new();

    /// <summary>
    /// Layers to add to the sprite of the player that is wearing this entity (for changing gun color)
    /// </summary>
    [DataField]
    public Dictionary<string, List<PrototypeLayerData>> ClothingVisuals = new();

}
