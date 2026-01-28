using Content.Shared.Alert;
using Content.Shared.Alert.Components;
using Content.Shared.FixedPoint;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._UM.Spiders.SpiderEnergy;

/// <summary>
/// This handles...
/// </summary>
public sealed class SpiderEnergySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly INetManager _net = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpiderEnergyComponent, GetGenericAlertCounterAmountEvent>(OnGetCounterAmount);
        SubscribeLocalEvent<SpiderEnergyComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<SpiderEnergyComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.NextUpdate > curTime)
                continue;

            if (comp.PassiveRegen != 0 && comp.Energy < comp.MaxRegenEnergy)
                TryAddEnergy(uid, comp.PassiveRegen);

            comp.NextUpdate += comp.UpdateInterval;
        }
    }

    private void OnMapInit(Entity<SpiderEnergyComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.NextUpdate;

        if (ent.Comp.IgnoreEnergy)
            return;

        _alerts.ShowAlert(ent.Owner, ent.Comp.EnergyAlert);
    }

    public bool TrySpendEnergy(Entity<SpiderEnergyComponent?> ent, FixedPoint2 amount)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (ent.Comp.IgnoreEnergy)
            return true;

        if (ent.Comp.Energy < amount)
            return false;

        if (_net.IsClient)
            return true;

        ent.Comp.Energy -= amount;
        Dirty(ent);
        _alerts.ShowAlert(ent.Owner, ent.Comp.EnergyAlert);
        return true;
    }

    public bool CanSpendEnergy(Entity<SpiderEnergyComponent?> ent, FixedPoint2 amount)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (ent.Comp.IgnoreEnergy)
            return true;

        if (ent.Comp.Energy < amount)
            return false;

        return true;
    }

    public bool TryAddEnergy(Entity<SpiderEnergyComponent?> ent, FixedPoint2 amount)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (ent.Comp.Energy + amount < 0)
            return false;

        if (_net.IsClient)
            return true;

        Log.Debug("Energy: " + ent.Comp.Energy);
        ent.Comp.Energy += amount;
        Dirty(ent);
        _alerts.ShowAlert(ent.Owner, ent.Comp.EnergyAlert);
        return true;
    }

    private void OnGetCounterAmount(Entity<SpiderEnergyComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.EnergyAlert != args.Alert)
            return;

        args.Amount = ent.Comp.Energy.Int();
    }
}
