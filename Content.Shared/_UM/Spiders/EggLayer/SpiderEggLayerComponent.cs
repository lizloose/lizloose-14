using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._UM.Spiders.EggLayer;

/// <summary>
/// This is used for...
/// </summary>
[AutoGenerateComponentState]
[RegisterComponent, NetworkedComponent]
public sealed partial class SpiderEggLayerComponent : Component
{
    [DataField]
    public EntProtoId LayEggAction = "ActionLayEggs";

    [DataField, AutoNetworkedField]
    public EntityUid? LayEggActionEntity;

    /// <summary>
    /// Proto to use for eggs
    /// </summary>
    [DataField]
    public EntProtoId EggProto = "BroodmotherEggs";

    /// <summary>
    /// How much energy should it cost to lay eggs
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public int LayEggCost = 20;

    [DataField]
    public string EnergyName = "spider";

    /// <summary>
    /// How much time it takes to lay an egg
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public TimeSpan LayEggTime = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Sound that should play when laying an egg
    /// </summary>
    [DataField]
    public SoundSpecifier LayEggSound = new SoundPathSpecifier("/Audio/Items/squeezebottle.ogg"); //the glue sound is fucked
}

public sealed partial class OnLayEggActionEvent : InstantActionEvent
{
}

[Serializable, NetSerializable]
public sealed partial class LayEggDoAfterEvent : SimpleDoAfterEvent
{
}
