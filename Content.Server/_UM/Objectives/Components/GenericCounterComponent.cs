namespace Content.Server._UM.Objectives.Components;

/// <summary>
/// This is used as a generic counter for objectives.
/// Requires NumberObjectiveComponent
/// </summary>
[RegisterComponent]
public sealed partial class GenericCounterComponent : Component
{
    [DataField]
    public int Count;
}
