using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._UM.Antags.Victim.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class VictimComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public TimeSpan DetonationTime;

    [DataField, ViewVariables]
    public TimeSpan BombDuration = TimeSpan.FromMinutes(2);

    /// <summary>
    /// How often the status effect should update
    /// </summary>
    [ViewVariables]
    public TimeSpan UpdateInterval = TimeSpan.FromMinutes(1);

    [ViewVariables]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Is the bomb enabled?
    /// </summary>
    [ViewVariables]
    public bool BombEnabled = false;

    [DataField]
    public ProtoId<AlertPrototype> TimerAlert = "VictimAlert";
}
