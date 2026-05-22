using System.Linq;
using Content.Client.Power.Components;
using Content.Client.Power.EntitySystems;
using Content.Shared.Audio;
using Content.Shared.Power.EntitySystems;
using Robust.Client.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Client._UM.Audio;

/// <summary>
/// This handles...
/// </summary>
public sealed class StationAmbienceSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private Entity<AudioComponent>? _playingSound;

    private readonly SoundSpecifier _soundFile = new SoundPathSpecifier("/Audio/Ambience/shipambience.ogg");

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_gameTiming.IsFirstTimePredicted)
            return;

        var player = _playerManager.LocalEntity;

        if (player is not { } ent)
            return;

        var powered = IsNearPower(ent);

        if (!powered && _playingSound != null)
        {
            _audio.Stop(_playingSound);
            _playingSound = null;
            return;
        }

        if (_playingSound == null && powered)
        {
            _playingSound = _audio.PlayGlobal(_soundFile,
                ent,
                audioParams: new AudioParams()
            {
                Volume = -8,
                Loop = true
            });
        }
    }

    public bool IsNearPower(EntityUid ent)
    {
        var xform = Transform(ent);

        if (xform.GridUid == null)
            return false;

        foreach (var uid in _lookup.GetEntitiesInRange(xform.Coordinates, 10f, LookupFlags.Static))
        {
            if (TryComp<ApcPowerReceiverComponent>(uid, out var comp) && comp.Powered)
            {
                return true;
            }
        }

        return false;
    }
}
