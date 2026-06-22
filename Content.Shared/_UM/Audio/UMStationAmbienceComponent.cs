using Content.Shared.EntityConditions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Audio;

/// <summary>
/// This is used for ambient sounds that are only audible while on the station grid
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class UMStationAmbienceComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public List<ProtoId<StationAmbiencePrototype>> Ambience = new();
}


[Prototype]
public sealed partial class StationAmbiencePrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = string.Empty;

    [ViewVariables(VVAccess.ReadWrite), DataField(required: true)]
    public SoundSpecifier Sound = default!;

    [DataField]
    public EntityCondition[]? Conditions;

    [DataField]
    public bool FadeOut = true;
}
