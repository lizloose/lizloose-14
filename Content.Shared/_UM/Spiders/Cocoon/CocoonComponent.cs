using Content.Shared.DoAfter;
using Content.Shared.Tools;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._UM.Spiders.Cocoon;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class CocoonComponent : Component
{
    /// <summary>
    /// The contents of the cocoon
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public Container Contents = default!;

    /// <summary>
    /// the ID of the container
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public string ContainerId = "contents";

    /// <summary>
    /// Sound that should play when laying an egg
    /// </summary>
    [DataField]
    public SoundSpecifier DestroySound = new SoundPathSpecifier("/Audio/Effects/poster_broken.ogg");

    /// <summary>
    /// Tool quality required to use a tool on this.
    /// </summary>
    [DataField]
    public ProtoId<ToolQualityPrototype> Quality = "Slicing";

    /// <summary>
    /// Time it takes to open the cocoon
    /// </summary>
    [DataField]
    public TimeSpan OpenTime = TimeSpan.FromSeconds(6);

    /// <summary>
    /// Whether or not tha cocoon has been harvested
    /// </summary>
    [DataField, AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool Harvested;

    /// <summary>
    /// How long should it take to absorb?
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Maximum energy this cocoon can provide
    /// Rolls on component init
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public int MaxEnergy = 70;

    /// <summary>
    /// minimum energy this cocoon can provide
    /// Rolls on component init
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public int MinEnergy = 40;

}


[Serializable, NetSerializable]
public sealed partial class OnCocoonDestroyDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class OnCocoonEnergyAbsorbDoAfterEvent : SimpleDoAfterEvent
{
}
