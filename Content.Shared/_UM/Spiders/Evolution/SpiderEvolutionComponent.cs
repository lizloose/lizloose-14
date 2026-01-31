using Content.Shared.Actions;
using Content.Shared.DoAfter;
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

    /// <summary>
    /// How much energy it costs to evolve
    /// </summary>
    [DataField, AutoNetworkedField]
    public int EvolutionCost = 50;

    [DataField]
    public string EnergyName = "spider";

    /// <summary>
    /// How long it should take to evolve
    /// </summary>
    [DataField]
    public TimeSpan EvolutionDuration = TimeSpan.FromSeconds(25);
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


[Serializable, NetSerializable]
public sealed partial class EvolveDoAfterEvent : DoAfterEvent
{
    public EntProtoId ProtoId;

    public override DoAfterEvent Clone() => this;
}
