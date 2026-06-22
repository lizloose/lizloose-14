using Content.Server.Atmos.EntitySystems;
using Content.Shared._UM.Atmos;
using Content.Shared.Atmos;

namespace Content.Server._UM.Atmos;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class UMAtmosPressurizedSystem : EntitySystem
{
    [Dependency] private AtmosphereSystem _atmosphereSystem = default!;

    private const float UpdateTimer = 1f;
    private float _timer;

    public override void Update(float frameTime)
    {
        _timer += frameTime;

        if (_timer < UpdateTimer)
            return;

        _timer -= UpdateTimer;

        var enumerator = EntityQueryEnumerator<UMAtmosPressurizedComponent>();
        while (enumerator.MoveNext(out var uid, out var pressurizedComponent))
        {

            var pressure = 0f;

            if (_atmosphereSystem.GetContainingMixture(uid) is {} mixture)
            {
                pressure = mixture.Pressure;
            }

            if (pressure <= Atmospherics.HazardLowPressure)
            {
                pressurizedComponent.Pressurized = false;
                Dirty(uid, pressurizedComponent);
                continue;
            }

            pressurizedComponent.Pressurized = true;
            Dirty(uid, pressurizedComponent);
        }
    }

}
