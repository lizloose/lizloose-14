using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._UM.Antags.Victim.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VictimComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public TimeSpan DetonationTime;

    [DataField, ViewVariables]
    public TimeSpan BombDuration = TimeSpan.FromMinutes(10);
}

[Serializable, NetSerializable]
public sealed class VictimTimerBoundUserInterfaceState : BoundUserInterfaceState
{
    public TimeSpan DetonationTime;

    public VictimTimerBoundUserInterfaceState(TimeSpan detonationTime)
    {
        DetonationTime = detonationTime;
    }

}


[Serializable, NetSerializable]
public enum VictimTimerUiKey
{
    Key,
}
