using Content.Server._UM.Antags.Objectives.Components;
using Content.Server.Damage.Components;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared._UM.Antags.Victim.Components;
using Content.Shared._UM.Antags.Victim.Systems;
using Content.Shared.Alert;
using Content.Shared.Mind;
using Robust.Shared.Timing;

namespace Content.Server._UM.Antags.Victim;

public sealed class VictimSystem : SharedVictimSystem
{

    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

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

            _alerts.ShowAlert(uid, comp.TimerAlert);

            comp.NextUpdate += comp.UpdateInterval;
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
