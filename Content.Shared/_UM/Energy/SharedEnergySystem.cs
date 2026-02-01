using Content.Shared._UM.Energy.Components;
using Content.Shared.Alert;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Shared._UM.Energy;

public sealed class SharedEnergySystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<EnergyComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<EnergyComponent, EntGotInsertedIntoContainerMessage>(OnEnergyInserted);
        SubscribeLocalEvent<EnergyComponent, EntGotRemovedFromContainerMessage>(OnEnergyRemoved);
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
            if (comp.Amount >= comp.MaxRegen || comp.PassiveRegen == 0)
                continue;
            comp.Amount = Math.Min(comp.MaxRegen, comp.Amount + comp.PassiveRegen);
            Dirty(uid, comp);
        }
    }

    private void OnEnergyInserted(Entity<EnergyComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (!TryComp<EnergyContainerComponent>(args.Container.Owner, out var energyContainer))
            return;

        if (ent.Comp.Alert != null)
            _alerts.ShowAlert(args.Container.Owner, ent.Comp.Alert.Value);

        energyContainer.EnergyTypes.TryAdd(ent.Comp.Name, ent);
    }

    private void OnEnergyRemoved(Entity<EnergyComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (!TryComp<EnergyContainerComponent>(args.Container.Owner, out var energyContainer))
            return;

        if (ent.Comp.Alert != null)
            _alerts.ClearAlert(args.Container.Owner, ent.Comp.Alert.Value);

        energyContainer.EnergyTypes.Remove(ent.Comp.Name);
    }

    private void OnMapInit(Entity<EnergyComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdate += _timing.CurTime + ent.Comp.UpdateInterval;
    }
}
