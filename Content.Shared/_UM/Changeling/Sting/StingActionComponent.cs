using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._UM.Changeling.Sting;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class StingActionComponent : Component
{
    /// <summary>
    /// The reagent(s) to be injected into the target.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public Solution Solution = new([new("Nocturine", 30)]);
}


[ByRefEvent]
public sealed partial class StingActionEvent : EntityTargetActionEvent;
