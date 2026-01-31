using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._UM.Energy;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class EnergyComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Amount { get; set; }

    [DataField, AutoNetworkedField]
    public int UpdateAmount { get; set; }

    [DataField, AutoNetworkedField]
    public int Max = 999;

    [DataField, AutoNetworkedField]
    public ProtoId<AlertPrototype>? Alert;

    [DataField, AutoNetworkedField]
    public EntityUid ContainerOwner;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    [AutoNetworkedField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(2);
}
