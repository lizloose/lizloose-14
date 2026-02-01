using Content.Shared.Actions;

namespace Content.Shared._UM.Changeling.Sting.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class ExtractGenomeActionComponent : Component
{
    [DataField]
    public bool Silent = true;

    [DataField]
    public LocId UserPopup = "changeling-extract-genome-sting-self";

    [DataField]
    public LocId TargetPopup = "injector-component-feel-prick-message";
}


[ByRefEvent]
public sealed partial class ExtractGenomeActionEvent : EntityTargetActionEvent;
