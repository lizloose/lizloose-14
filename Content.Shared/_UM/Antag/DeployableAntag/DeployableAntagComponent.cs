using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._UM.Antag.DeployableAntag;

/// <summary>
/// This handles antagonists that start off as a normal mob, find a safe place, and transform into the actual antag.
/// Useful for things like blob, broodmother, chest bursters, etc
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class DeployableAntagComponent : Component
{
    [DataField]
    public EntProtoId DeployAction = "ActionDeployBroodmother";

    [DataField, AutoNetworkedField]
    public EntityUid? DeployActionEntity;

    /// <summary>
    /// What mob should we turn into?
    /// </summary>
    [DataField]
    public EntProtoId ProtoId = "MobBroodmother";

    /// <summary>
    /// What mob should we turn into?
    /// </summary>
    [DataField]
    public TimeSpan DeployTime = TimeSpan.FromSeconds(7);

    /// <summary>
    /// Locale id of the doafter message
    /// </summary>
    [DataField]
    public LocId DeployMessage = "spider-deployed";

    /// <summary>
    /// Do we gib the original mob when the new one deploys?
    /// </summary>
    [DataField]
    public bool GibOriginal = true;
}


public sealed partial class DeployAntagActionEvent : InstantActionEvent
{
}


[Serializable, NetSerializable]
public sealed partial class AntagDeployDoAfterEvent : SimpleDoAfterEvent
{
}
