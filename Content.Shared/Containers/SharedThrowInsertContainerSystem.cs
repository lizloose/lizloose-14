namespace Content.Shared.Containers;

/// <summary>
/// Sent before the insertion is made.
/// Allows preventing the insertion if any system on the entity should need to.
/// </summary>
[ByRefEvent]
public record struct BeforeThrowInsertEvent(EntityUid ThrownEntity, bool Cancelled = false);

//BEGIN UM
/// <summary>
/// Sent after the insertion is made
/// </summary>
[ByRefEvent]
public record struct AfterThrowInsertEvent(EntityUid ThrownEntity, EntityUid Thrower);
//END UM
