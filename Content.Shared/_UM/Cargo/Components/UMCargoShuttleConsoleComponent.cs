using Content.Shared.Shuttles.Systems;
using Content.Shared.Timing;
using Robust.Shared.Serialization;

namespace Content.Shared._UM.Cargo.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class UMCargoShuttleConsoleComponent : Component
{
    [ViewVariables]
    public EntityUid ShuttleUid;

}

[Serializable, NetSerializable]
public enum UMCargoShuttleUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class UMCargoShuttleBoundUserInterfaceState(FTLState ftlState, StartEndTime? stateTime, NetEntity? location) : BoundUserInterfaceState
{
    public FTLState FTLState = ftlState;

    public StartEndTime? StateTime = stateTime;

    public NetEntity? Location = location;
}

[Serializable, NetSerializable]
public sealed class UMSendCargoShuttleMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public enum ShuttleLocation
{
    Station,
    Moving,
    Centcom,
}
