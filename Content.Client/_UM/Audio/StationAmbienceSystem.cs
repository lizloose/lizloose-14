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
public sealed class StationAmbienceSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ContentAudioSystem _contentAudio = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedStationSystem _station = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    [Dependency] private readonly SharedEntityConditionsSystem _conditions = default!;

    private Dictionary<StationAmbiencePrototype, Entity<AudioComponent>?> _playingSounds = new();

    private Entity<AudioComponent>? _playingSound = null;

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

        if (_playerManager.LocalEntity is not { } player)
            return;

        var xform = Transform(player);

        if (_station.GetOwningStation(xform.GridUid) is not { } station)
        {
            foreach (var (proto, sound) in _playingSounds)
            {
                _audio.Stop(sound);
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
                _contentAudio.FadeOut(sound);
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
                _playingSounds[proto] = _audio.PlayGlobal(proto.Sound, player);
            }
        }
    }
}
