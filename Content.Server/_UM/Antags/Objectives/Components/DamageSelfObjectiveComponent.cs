using Content.Shared.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Server._UM.Antags.Objectives.Components;

/// <summary>
/// This is used for keeping track of the objective for dealing damage to yourslef
/// </summary>
[RegisterComponent]
public sealed partial class DamageSelfObjectiveComponent : Component
{
    /// <summary>
    /// Minimum amount of damage to be picked. Will roll on init
    /// </summary>
    [DataField]
    public int MinDamage = 200;

    /// <summary>
    /// Minimum amount of damage to be picked. Will roll on init
    /// </summary>
    [DataField]
    public int MaxDamage = 400;

    /// <summary>
    /// How much damage should you have to deal to yourself?
    /// </summary>
    [ViewVariables]
    public FixedPoint2 Damage;

    /// <summary>
    /// Total amount of self inflicted pain
    /// </summary>
    [ViewVariables]
    public FixedPoint2 DamageDealt;
}
