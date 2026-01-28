using Content.Shared.Actions;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._UM.Spiders.Evolution;

/// <summary>
/// This handles spider evolutions
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class SpiderEvolutionComponent : Component
{
    [DataField]
    public EntProtoId RadialAction = "ActionSpiderEvolve";

    [DataField, AutoNetworkedField]
    public EntityUid? RadialActionEntity;

    [DataField, AutoNetworkedField]
    public List<EntProtoId> EvolutionTypes = new() { "MobLizard", "MobMonkey", "MobGorilla" };

    [DataField, AutoNetworkedField]
    public FixedPoint2 EvolutionCost = 50;
}

[Serializable, NetSerializable]
public sealed class SpiderEvolutionSelectMessage(EntProtoId protoId) : BoundUserInterfaceMessage
{
    public readonly EntProtoId PrototypeId = protoId;
}

[Serializable, NetSerializable]
public enum SpiderEvolveRadialUiKey : byte
{
    Key,
}

/// <summary>
/// Action event for evolving
/// </summary>
public sealed partial class SpiderEvolveActionEvent : InstantActionEvent;
