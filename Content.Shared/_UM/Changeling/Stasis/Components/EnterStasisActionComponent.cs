using Content.Shared.Actions;

namespace Content.Shared._UM.Changeling.Stasis.Components;

/// <summary>
/// This is used for handling the Changeling stasis action
/// </summary>
[RegisterComponent]
public sealed partial class EnterStasisActionComponent : Component
{
}


[ByRefEvent]
public sealed partial class StasisEnterActionEvent : InstantActionEvent;


