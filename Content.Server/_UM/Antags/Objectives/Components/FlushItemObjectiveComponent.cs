using Robust.Shared.Prototypes;

namespace Content.Server._UM.Antags.Objectives.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class FlushItemObjectiveComponent : Component
{
    [DataField(required: true)]
    public List<EntProtoId> Targets = new();

    [ViewVariables]
    public EntityPrototype Target;

    [ViewVariables]
    public bool ItemInserted = false;

    [ViewVariables]
    public bool BinFlushed = false;
}
