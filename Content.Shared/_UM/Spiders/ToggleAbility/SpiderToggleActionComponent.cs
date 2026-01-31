using Content.Shared.Actions.Components;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Toggleable;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._UM.Spiders.ToggleAbility;

/// <summary>
/// Used for abilities that toggle things.
/// Requires <see cref="ItemToggleComponent"/> and <see cref="SpiderEnergyComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class SpiderToggleActionComponent : Component
{
    /// <summary>
    /// The action to add when equipped, even if not worn.
    /// This must raise <see cref="ToggleActionEvent"/> to then get handled.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId<InstantActionComponent> Action;

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    /// <summary>
    /// How many points per tick does this ability cost to use
    /// </summary>
    [DataField, AutoNetworkedField]
    public int EnergyDrain = 5;

    [DataField]
    public string EnergyName = "spider";

    /// <summary>
    /// Time when next passive energy regen update will happen.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    /// How frequently will regeneration occur
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(2);

}
