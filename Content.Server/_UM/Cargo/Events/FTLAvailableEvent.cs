namespace Content.Server._UM.Cargo.Events;

/// <summary>
/// Raised when the FTL Cooldown is over
/// </summary>
[ByRefEvent]
public readonly record struct FTLAvailableEvent(EntityUid Entity);
