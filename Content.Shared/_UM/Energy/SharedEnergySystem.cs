using Content.Shared.Alert;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Shared._UM.Energy;

/// <summary>
/// This handles...
/// </summary>
public sealed class SharedEnergySystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<EnergyComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<EnergyComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (curTime < comp.NextUpdate)
                continue;

            comp.NextUpdate += comp.UpdateInterval;
            if (comp.Amount >= comp.MaxRegen)
                continue;
            comp.Amount = Math.Min(comp.MaxRegen, comp.Amount + comp.PassiveRegen);
            Dirty(uid, comp);

            if (comp.Alert != null)
                _alerts.ShowAlert(comp.ContainerOwner, comp.Alert.Value);
        }
    }


    private void OnMapInit(Entity<EnergyComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdate += _timing.CurTime + ent.Comp.UpdateInterval;

        if (ent.Comp.Alert == null)
            return;

        _alerts.ShowAlert(ent.Comp.ContainerOwner, ent.Comp.Alert.Value);
    }
}
