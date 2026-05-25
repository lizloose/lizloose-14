using System.Linq;
using Content.Client.Power.Components;
using Content.Client.Power.EntitySystems;
using Content.Shared._UM.Audio;
using Content.Shared.Audio;
using Content.Shared.EntityConditions;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Station;
using Robust.Client.Player;
using Robust.Shared.Audio;
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
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedStationSystem _station = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    [Dependency] private readonly SharedEntityConditionsSystem _conditions = default!;

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
            _audio.Stop(_playingSound);
            _playingSound = null;
            return;
        }

        if (!TryComp<StationAmbienceComponent>(station, out var comp))
            return;

        if (_playingSound is not null && !Exists(_playingSound))
            _playingSound = null;

        foreach (var sound in comp.Ambience)
        {
            if (!_prototypeManager.Resolve(sound, out var proto))
                continue;

            var passed = _conditions.TryConditions(player, proto.Conditions);

            if (!passed && _playingSound != null)
            {
                _audio.Stop(_playingSound);
                _playingSound = null;
            }

            if (passed && _playingSound == null)
            {
                _playingSound = _audio.PlayGlobal(proto.Sound, player);
            }
        }
    }
}
