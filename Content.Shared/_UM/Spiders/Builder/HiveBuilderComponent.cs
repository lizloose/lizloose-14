using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._UM.Spiders.Builder;

/// <summary>
/// This handles spiders building the hive
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class HiveBuilderComponent : Component
{
    [DataField]
    public EntProtoId RadialAction = "ActionSpiderSelectBuild";

    [DataField, AutoNetworkedField]
    public EntityUid? RadialActionEntity;

    [DataField]
    public EntProtoId BuildAction = "ActionSpiderBuildSelected";

    [DataField, AutoNetworkedField]
    public EntityUid? BuildActionEntity;

    /// <summary>
    /// Things the spider can build
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public List<ProtoId<HiveBuildPrototype>> BuildTypes = new();

    [DataField]
    public TimeSpan BuildTime = TimeSpan.FromSeconds(0.5);

    [DataField]
    public string EnergyName = "spider";

    /// <summary>
    /// Currently selected build.
    /// </summary>
    [ViewVariables]
    public ProtoId<HiveBuildPrototype> CurrentBuild = new();
}



[Serializable, NetSerializable]
public sealed class HiveBuilderTypeSelectMessage(ProtoId<HiveBuildPrototype> protoId) : BoundUserInterfaceMessage
{
    public readonly ProtoId<HiveBuildPrototype> PrototypeId = protoId;
}

[Serializable, NetSerializable]
public enum HiveBuilderRadialUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed partial class OnBuildDoAfterEvent : DoAfterEvent
{
    public NetCoordinates TargetCoordinates;

    public override DoAfterEvent Clone() => this;
}


/// <summary>
/// Action event for opening the hive builders radial menu.
/// </summary>
public sealed partial class HiveBuilderSelectTypeActionEvent : InstantActionEvent;

/// <summary>
/// Action event for building the selected thing
/// </summary>
public sealed partial class HiveBuilderBuildActionEvent : WorldTargetActionEvent;
