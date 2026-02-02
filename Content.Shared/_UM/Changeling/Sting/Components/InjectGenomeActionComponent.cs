using Content.Shared.Actions;
using Content.Shared.Cloning;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Changeling.Sting.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class InjectGenomeActionComponent : Component
{
    /// <summary>
    /// The cloning settings to use.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<CloningSettingsPrototype> Settings = "ChangelingCloningSettings";
}


[ByRefEvent]
public sealed partial class InjectGenomeActionEvent : EntityTargetActionEvent;
