using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._UM.Spiders;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class MobSizeComponent : Component
{
    /// <summary>
    /// What size is this mob?
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public MobSizes Size = MobSizes.Small;
}

[Serializable, NetSerializable]
public enum MobSizes : byte
{
    Small,
    Humanoid
}
