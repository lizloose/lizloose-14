using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._UM.Spiders.SpiderMender;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class SpiderMenderComponent : Component
{
    [DataField]
    public EntProtoId MendAction = "ActionWebMend";

    [DataField, AutoNetworkedField]
    public EntityUid? MendActionEntity;

    /// <summary>
    /// How much energy does it cost to heal
    /// </summary>
    [DataField]
    public FixedPoint2 HealCost = 5;

    /// <summary>
    /// How much should the action heal
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier HealAmount;

    /// <summary>
    /// Entity whitelist for what is allowed to be healed.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// How long it takes for the heal doafter
    /// </summary>
    [DataField]
    public TimeSpan HealTime = TimeSpan.FromSeconds(8);
}


public sealed partial class OnSpiderMendActionEvent : EntityTargetActionEvent
{
}

[Serializable, NetSerializable]
public sealed partial class SpiderMendDoAfterEvent : SimpleDoAfterEvent
{
}
