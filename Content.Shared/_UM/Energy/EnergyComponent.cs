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
    [DataField(required: true), AutoNetworkedField]
    public Dictionary<string, Energy> Types = new();

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    [AutoNetworkedField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(2);
}


[Serializable, NetSerializable, DataDefinition]
public partial struct Energy : IRobustCloneable<Energy>
{
    [DataField]
    public int Amount { get; set; }

    [DataField]
    public int UpdateAmount { get; set; }

    [DataField]
    public int Max = 999;

    [DataField]
    public ProtoId<AlertPrototype>? Alert = "Essence";

    public Energy(Energy energy)
    {
        Amount = energy.Amount;
        UpdateAmount = energy.UpdateAmount;
    }

    public Energy Clone()
    {
        return new Energy(this);
    }
}
