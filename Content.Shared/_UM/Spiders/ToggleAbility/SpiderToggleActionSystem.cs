using Content.Shared._UM.Energy;
using Content.Shared.Actions;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Toggleable;
using Robust.Shared.Timing;

namespace Content.Shared._UM.Spiders.ToggleAbility;

public sealed class SpiderToggleActionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly EnergyContainerSystem _energy = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpiderToggleActionComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SpiderToggleActionComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SpiderToggleActionComponent, ToggleActionEvent>(OnToggleAction);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<SpiderToggleActionComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.NextUpdate > curTime)
                continue;

            if (_toggle.IsActivated(uid) && !_energy.TrySpendEnergy(uid, comp.EnergyName, comp.EnergyDrain))
                _toggle.Toggle(uid);

            comp.NextUpdate += comp.UpdateInterval;
        }
    }

    private void OnMapInit(Entity<SpiderToggleActionComponent> ent, ref MapInitEvent args)
    {
        _actionsSystem.AddAction(ent, ref ent.Comp.ActionEntity, ent.Comp.Action);

    }

    private void OnShutdown(Entity<SpiderToggleActionComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.ActionEntity != null)
        {
            _actionsSystem.RemoveAction(ent.Owner, ent.Comp.ActionEntity);
        }
    }

    private void OnToggleAction(Entity<SpiderToggleActionComponent> ent, ref ToggleActionEvent args)
    {
        if (!_toggle.IsActivated(ent.Owner) && _energy.CanSpendEnergy(ent.Owner, ent.Comp.EnergyName, ent.Comp.EnergyDrain))
        {
            args.Handled = _toggle.Toggle(ent.Owner, args.Performer);
            return;
        }

        if (_toggle.IsActivated(ent.Owner))
            args.Handled = _toggle.Toggle(ent.Owner, args.Performer);
    }

}
