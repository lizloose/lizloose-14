using Content.Shared.Actions;

namespace Content.Shared._UM.Spiders.SpiderEyes;

/// <summary>
/// This handles...
/// </summary>
public abstract class SharedSpiderEyesSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpiderEyesComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<SpiderEyesComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.ToggleLightingActionEntity, ent.Comp.ToggleLightingAction);
        Dirty(ent);
    }
}
