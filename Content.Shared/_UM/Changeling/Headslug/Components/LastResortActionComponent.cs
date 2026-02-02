using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Changeling.Headslug.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class LastResortActionComponent : Component
{
    [DataField]
    public EntProtoId ProtoId = "MobHeadslug";
}


[ByRefEvent]
public sealed partial class LastResortActionEvent : InstantActionEvent;
