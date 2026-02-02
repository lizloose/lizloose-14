using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._UM.Changeling.Stasis.Components;

/// <summary>
/// This component is used for changelings that are currently in stasis.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class RevivingStasisComponent : Component
{
    /// <summary>
    /// If true, adds the GhostOnMove component to the user when they leave stasis
    /// used to prevent them from entering stasis and ghosting around
    /// </summary>
    [DataField]
    public bool GhostOnMove;

    [DataField]
    public bool StasisFinished;

    [DataField]
    public TimeSpan Duration = TimeSpan.FromMinutes(1);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan StasisEnd = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity = null;

    [DataField]
    public ProtoId<AlertPrototype> StasisAlert = "StasisRegenerating";

    [DataField]
    public ProtoId<AlertPrototype> StasisReadyAlert = "StasisReady";
}


/// <summary>
///     Raised when you click on the Revive Stasis Alert
/// </summary>
public sealed partial class StasisReviveAlertEvent : BaseAlertEvent;
