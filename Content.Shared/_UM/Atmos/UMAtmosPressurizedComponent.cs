using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._UM.Atmos;

/// <summary>
/// This is used for tracking if the entity is in a pressurized area
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class UMAtmosPressurizedComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public bool Pressurized = false;
}
