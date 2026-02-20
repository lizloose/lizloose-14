using Content.Shared.Actions;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._UM.Changeling.Devour.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class UMChangelingDevourActionComponent : Component
{
    /// <summary>
    /// The whitelist of targets for devouring
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityWhitelist? Whitelist = new()
    {
        Components =
        [
            "MobState",
            "HumanoidProfile",
        ],
    };

    /// <summary>
    /// The Sound to use during consumption of a victim
    /// </summary>
    /// <remarks>
    /// 6 distance due to the default 15 being hearable all the way across PVS. Changeling is meant to be stealthy.
    /// 6 still allows the sound to be hearable, but not across an entire department.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? ConsumeNoise = new SoundCollectionSpecifier("ChangelingDevourConsume", AudioParams.Default.WithMaxDistance(6));

    /// <summary>
    /// The Sound to use during the windup before consuming a victim
    /// </summary>
    /// <remarks>
    /// 6 distance due to the default 15 being hearable all the way across PVS. Changeling is meant to be stealthy.
    /// 6 still allows the sound to be hearable, but not across an entire department.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? DevourWindupNoise = new SoundCollectionSpecifier("ChangelingDevourWindup", AudioParams.Default.WithMaxDistance(6));


    /// <summary>
    /// The windup time before the changeling begins to engage in devouring the identity of a target
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan DevourWindupTime = TimeSpan.FromSeconds(2);

    /// <summary>
    /// The time it takes to FULLY consume someones identity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan DevourConsumeTime = TimeSpan.FromSeconds(10);

    /// <summary>
    /// The Currently active devour sound in the world
    /// </summary>
    [DataField]
    public EntityUid? CurrentDevourSound;

    /// <summary>
    /// The list of protective damage types capable of preventing a devour if over the threshold
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<ProtoId<DamageTypePrototype>> ProtectiveDamageTypes = new()
    {
        "Slash",
        "Piercing",
        "Blunt",
    };

    /// <summary>
    /// The percentage of ANY brute damage resistance that will prevent devouring
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DevourPreventionPercentageThreshold = 0.1f;

}


/// <summary>
/// Action event for Devour, someone has initiated a devour on someone, begin to windup.
/// </summary>
[ByRefEvent]
public sealed partial class UMChangelingDevourActionEvent : EntityTargetActionEvent;

/// <summary>
/// A windup has either successfully been completed or has been canceled. If successful start the devouring DoAfter.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class UMChangelingDevourWindupDoAfterEvent : DoAfterEvent
{
    public TimeSpan DevourConsumeTime;

    public SoundSpecifier? ConsumeNoise;

    public UMChangelingDevourWindupDoAfterEvent(TimeSpan devourConsumeTime, SoundSpecifier? consumeNoise)
    {
        DevourConsumeTime = devourConsumeTime;
        ConsumeNoise = consumeNoise;
    }

    public override DoAfterEvent Clone() => this;
}

/// <summary>
/// The Consumption DoAfter has either successfully been completed or was canceled.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class UMChangelingDevourConsumeDoAfterEvent : SimpleDoAfterEvent;
