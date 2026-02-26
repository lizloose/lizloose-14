using Content.Shared.Shuttles.Systems;
using Content.Shared.Timing;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._UM.Cargo.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class UMCargoShuttleConsoleComponent : Component
{
    [ViewVariables]
    public EntityUid ShuttleUid;

    /// <summary>
    /// Sound played when console is mad
    /// </summary>
    [DataField]
    public SoundSpecifier ErrorSound = new SoundCollectionSpecifier("CargoError");

    /// <summary>
    /// The time at which the console will be able to play the deny sound.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextDenySoundTime = TimeSpan.Zero;

    /// <summary>
    /// The time between playing the deny sound.
    /// </summary>
    [DataField]
    public TimeSpan DenySoundDelay = TimeSpan.FromSeconds(2);

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
