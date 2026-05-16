using Content.Shared.Whitelist;

namespace Content.Server._UM.Antags.Objectives.Components;

/// <summary>
/// This is the objective component that handles welder bombing the hop
/// </summary>
[RegisterComponent]
public sealed partial class WelderBombObjectiveComponent : Component
{
    /// <summary>
    /// Warp point that the welder bomb has to target
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? Target;

    /// <summary>
    /// Tags that should be used to exclude Warp Points
    /// from the list of valid bombing targets
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;

    /// <summary>
    /// Range for how far we can welder bomb
    /// </summary>
    [DataField]
    public float Range = 12f;

    /// <summary>
    /// Whether or not they did the bomb
    /// </summary>
    [DataField]
    public bool Bombed = false;
}
