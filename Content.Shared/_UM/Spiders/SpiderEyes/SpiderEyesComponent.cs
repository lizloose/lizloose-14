using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Spiders.SpiderEyes;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class SpiderEyesComponent : Component
{
    [DataField]
    public EntProtoId ToggleLightingAction = "ActionSpiderEyes";

    [DataField, AutoNetworkedField]
    public EntityUid? ToggleLightingActionEntity;
}


public sealed partial class ToggleSpiderEyesActionEvent : InstantActionEvent;
