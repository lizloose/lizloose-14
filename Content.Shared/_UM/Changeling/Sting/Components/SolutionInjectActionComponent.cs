using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._UM.Changeling.Sting.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class SolutionInjectActionComponent : Component
{
    /// <summary>
    /// The reagent(s) to be injected into the target.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public Solution Solution = new([new("Nocturine", 30)]);

    [DataField]
    public bool Silent = true;

    [DataField]
    public LocId UserPopup = "changeling-solution-inject-sting-stealth-self";

    [DataField]
    public LocId TargetPopup = "injector-component-feel-prick-message";

}


[ByRefEvent]
public sealed partial class SolutionInjectActionEvent : EntityTargetActionEvent;
