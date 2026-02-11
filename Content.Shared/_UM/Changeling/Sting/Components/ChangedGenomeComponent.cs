using Content.Shared.Cloning;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._UM.Changeling.Sting.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class ChangedGenomeComponent : Component
{

    [DataField, AutoNetworkedField]
    public EntityUid? OriginalEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? TransformedEntity;

    /// <summary>
    /// The cloning settings to use.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<CloningSettingsPrototype> Settings = "ChangelingCloningSettings";

    /// <summary>
    /// The server time at which the player will revert
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan EndTime = TimeSpan.Zero;

    /// <summary>
    /// How long the player will be transformed
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public TimeSpan Duration = TimeSpan.FromMinutes(0.2);

}
