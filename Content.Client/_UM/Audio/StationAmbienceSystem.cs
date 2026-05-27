using Content.Client.Audio;
using Content.Shared._UM.Audio;
using Content.Shared.EntityConditions;
using Content.Shared.Station;
using Robust.Client.Player;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._UM.Audio;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class StationAmbienceSystem : EntitySystem
{
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private ContentAudioSystem _contentAudio = default!;
    [Dependency] private IGameTiming _gameTiming = default!;
    [Dependency] private SharedStationSystem _station = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private SharedEntityConditionsSystem _conditions = default!;


    private Dictionary<StationAmbiencePrototype, Entity<AudioComponent>?> _playingSounds = new();

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_gameTiming.IsFirstTimePredicted)
            return;

        if (_playerManager.LocalEntity is not { } player)
            return;

        var xform = Transform(player);

        if (_station.GetOwningStation(xform.GridUid) is not { } station)
        {
            foreach (var (proto, sound) in _playingSounds)
            {
                StopSound(sound, proto.FadeOut);
                _playingSounds.Remove(proto);
            }
            return;
        }

        if (!TryComp<StationAmbienceComponent>(station, out var comp))
            return;

        //Check playing sounds
        foreach (var (proto, sound) in _playingSounds)
        {
            if (!Exists(sound))
            {
                _playingSounds.Remove(proto);
                continue;
            }

            if (!_conditions.TryConditions(player, proto.Conditions))
            {
                StopSound(sound, proto.FadeOut);
                _playingSounds.Remove(proto);
            }
        }

        //Check rules for not playing sounds
        foreach (var sound in comp.Ambience)
        {
            if (!_prototypeManager.Resolve(sound, out var proto))
                continue;

            if (_playingSounds.ContainsKey(proto))
                continue;

            if (_conditions.TryConditions(player, proto.Conditions))
            {
                var stream = _audio.PlayGlobal(proto.Sound, player);
                _playingSounds[proto] = stream;
            }
        }
    }

    private void StopSound(Entity<AudioComponent>? sound, bool fade)
    {
        if (fade)
        {
            _contentAudio.FadeOut(sound);
            return;
        }
        _audio.Stop(sound);
    }
}
