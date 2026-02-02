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

    [DataField]
    public LocId UserPopup = "changeling-solution-inject-sting-stealth-self";

    [DataField]
    public LocId TargetPopup = "injector-component-feel-prick-message";
}


[ByRefEvent]
public sealed partial class InjectGenomeActionEvent : EntityTargetActionEvent;
