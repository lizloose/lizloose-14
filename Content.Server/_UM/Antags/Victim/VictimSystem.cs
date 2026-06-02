using Content.Server.Objectives.Components;
using Content.Shared._UM.Antags.Victim.Components;
using Content.Shared._UM.Antags.Victim.Systems;
using Content.Shared.Alert;
using Content.Shared.Explosion.EntitySystems;
using Content.Shared.Mind;
using Robust.Shared.Timing;

namespace Content.Server._UM.Antags.Victim;

public sealed partial class VictimSystem : SharedVictimSystem
{
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedExplosionSystem _explosionSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VictimComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<VictimComponent, ComponentRemove>(ComponentRemoved);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Get the current server time.
        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<VictimComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.NextUpdate > curTime)
                continue;

            if (comp.BombEnabled)
                _alerts.ShowAlert(uid, comp.TimerAlert);
            else
                _alerts.ClearAlert(uid, comp.TimerAlert);

            if (comp.DetonationTime < curTime && comp.BombEnabled)
            {
                Detonate((uid, comp));
                continue;
            }
            comp.NextUpdate += comp.UpdateInterval;
        }
    }

    private void Detonate(Entity<VictimComponent> ent)
    {
        if (!_mind.TryGetMind(ent, out _, out var mindComponent))
            return;

        foreach (var objective in mindComponent.Objectives)
        {
            if (!TryComp<CodeConditionComponent>(objective, out var condition))
                continue;

            //minibomb
            //when offmed is in remove their head
            if (!condition.Completed)
            {
                _explosionSystem.QueueExplosion(ent.Owner, "Minibomb", 200, 30f, 60f, canCreateVacuum: true);
                _alerts.ClearAlert(ent.Owner, ent.Comp.TimerAlert);
                ent.Comp.BombEnabled = false;
                return;
            }

            ent.Comp.BombEnabled = false;
            _alerts.ClearAlert(ent.Owner, ent.Comp.TimerAlert);
        }
    }

    private void OnStartup(Entity<VictimComponent> ent, ref ComponentStartup args)
    {
        _alerts.ShowAlert(ent.Owner, ent.Comp.TimerAlert);
    }

    private void ComponentRemoved(Entity<VictimComponent> ent, ref ComponentRemove args)
    {
        _alerts.ClearAlert(ent.Owner, ent.Comp.TimerAlert);
    }
}
