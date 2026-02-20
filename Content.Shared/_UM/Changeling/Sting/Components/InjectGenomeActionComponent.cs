using Content.Shared.Actions;
using Content.Shared.Cloning;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Changeling.Sting.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class InjectGenomeActionComponent : Component
{
    /// <summary>
    /// The cloning settings to use.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<CloningSettingsPrototype> Settings = "ChangelingCloningSettings";

    [DataField]
    public bool Silent = true;

    /// <summary>
    /// How long should they be transformed for?
    /// Will revert back after this time.
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromMinutes(3);

    /// <summary>
    /// Popup when the target currently transformed by this action
    /// </summary>
    [DataField]
    public LocId AlreadyChangedPopup = "changeling-solution-inject-sting-stealth-self";

    /// <summary>
    /// Popup to be sent to the user doing the action when it succeeds
    /// </summary>
    [DataField]
    public LocId UserPopup = "changeling-solution-inject-sting-stealth-self";

    /// <summary>
    /// Popup to be sent to the target when the action succeeds
    /// Only when silent is false
    /// </summary>
    [DataField]
    public LocId TargetPopup = "injector-component-feel-prick-message";
}


[ByRefEvent]
public sealed partial class InjectGenomeActionEvent : EntityTargetActionEvent;
