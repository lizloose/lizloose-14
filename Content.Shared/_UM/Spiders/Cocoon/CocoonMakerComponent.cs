using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._UM.Spiders.Cocoon;

/// <summary>
/// Given to mobs that can trap people in cocoons
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class CocoonMakerComponent : Component
{
    [DataField]
    public EntProtoId CocoonAction = "ActionCocoon";

    [DataField, AutoNetworkedField]
    public EntityUid? CocoonActionEntity;

    /// <summary>
    /// Entity to use for large cocoons (containing people)
    /// </summary>
    [DataField]
    public EntProtoId LargeCocoon = "SpiderCocoonLarge";

    /// <summary>
    /// Entity to use for small cocoons (smaller mobs)
    /// </summary>
    [DataField]
    public EntProtoId SmallCocoon = "SpiderCocoonSmall";

    /// <summary>
    /// How long it should take to wrap someone in a cocoon
    /// TODO: Move this to mobsize when thats done
    /// </summary>
    [DataField]
    public TimeSpan WrapTime = TimeSpan.FromSeconds(5);

    [DataField]
    public string EnergyName = "spider";

    /// <summary>
    /// Damage to deal to person inside cocoon when their essence is absorbed.
    /// </summary>
    [DataField(required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new()
        {
            { "Cellular", 100 }
        }
    };
}


public sealed partial class OnCocoonWrapActionEvent : EntityTargetActionEvent
{
}

[Serializable, NetSerializable]
public sealed partial class OnCocoonWrapDoAfterEvent : SimpleDoAfterEvent
{
}
