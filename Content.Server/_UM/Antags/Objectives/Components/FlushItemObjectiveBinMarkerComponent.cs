using Robust.Shared.Serialization;

namespace Content.Server._UM.Antags.Objectives.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class FlushItemObjectiveBinMarkerComponent : Component
{
    public EntityUid Item;
    public EntityUid ObjectiveOwner;
}
