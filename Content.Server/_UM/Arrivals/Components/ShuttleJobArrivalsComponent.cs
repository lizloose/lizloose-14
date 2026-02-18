using Robust.Shared.Utility;

namespace Content.Server._UM.Arrivals.Components;

/// <summary>
/// This is used for handling the tider shuttle, the rounstart vessel that delivers tiders
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class ShuttleJobArrivalsComponent : Component
{
    /// <summary>
    /// How long the initial flight should take
    /// </summary>
    [DataField]
    public TimeSpan FlightDelay = TimeSpan.FromSeconds(240f);

    /// <summary>
    /// How long to wait at station until it leaves
    /// </summary>
    [DataField]
    public TimeSpan RestDelay = TimeSpan.FromSeconds(60f);

    [DataField, AutoPausedField]
    public TimeSpan TakeoffTime;

    [DataField]
    public EntityUid Station;
}
