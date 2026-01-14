using Content.Shared._UM.Antags.Victim.Components;
using Robust.Shared.Timing;

namespace Content.Shared._UM.Antags.Victim.Systems;

/// <summary>
/// This handles...
/// </summary>
public abstract partial class SharedVictimSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VictimComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<VictimComponent, ComponentRemove>(ComponentRemoved);
    }


    private void OnMapInit(Entity<VictimComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.DetonationTime = _gameTiming.CurTime + ent.Comp.BombDuration;

        _uiSystem.SetUiState(ent.Owner, VictimTimerUiKey.Key, new VictimTimerBoundUserInterfaceState(ent.Comp.DetonationTime));
        _uiSystem.SetUi(ent.Owner, VictimTimerUiKey.Key, new InterfaceData("VictimTimerBoundUserInterface"));
        _uiSystem.TryOpenUi(ent.Owner, VictimTimerUiKey.Key, ent.Owner);
    }

    private void ComponentRemoved(Entity<VictimComponent> ent, ref ComponentRemove args)
    {
        _uiSystem.CloseUi(ent.Owner, VictimTimerUiKey.Key);
    }
}
