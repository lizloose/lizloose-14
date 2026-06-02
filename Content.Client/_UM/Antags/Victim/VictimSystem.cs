using Content.Shared._UM.Antags.Victim.Components;
using Content.Shared._UM.Antags.Victim.Systems;
using Content.Shared.Alert.Components;
using Robust.Shared.Timing;

namespace Content.Client._UM.Antags.Victim;

/// <inheritdoc/>
public sealed partial class VictimSystem : SharedVictimSystem
{
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VictimComponent, GetGenericAlertCounterAmountEvent>(OnGetCounterAmount);
    }

    private void OnGetCounterAmount(Entity<VictimComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.TimerAlert != args.Alert)
            return;

        var timeLeft = ent.Comp.DetonationTime - _timing.CurTime;

        args.Amount = timeLeft.Minutes;
    }
}
