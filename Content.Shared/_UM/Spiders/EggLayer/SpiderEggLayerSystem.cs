using Content.Shared._UM.Energy;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._UM.Spiders.EggLayer;

/// <summary>
/// This handles...
/// </summary>
public sealed class EggLayerSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly EnergyContainerSystem _energy = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpiderEggLayerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SpiderEggLayerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SpiderEggLayerComponent, OnLayEggActionEvent>(OnLayEggAction);
    }

    private void OnMapInit(Entity<SpiderEggLayerComponent> ent, ref MapInitEvent args)
    {
        _actionsSystem.AddAction(ent, ref ent.Comp.LayEggActionEntity, ent.Comp.LayEggAction);
    }

    private void OnShutdown(Entity<SpiderEggLayerComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.LayEggActionEntity != null)
        {
            _actionsSystem.RemoveAction(ent.Owner, ent.Comp.LayEggActionEntity);
        }
    }

    private void OnLayEggAction(Entity<SpiderEggLayerComponent> ent, ref OnLayEggActionEvent args)
    {
        if (args.Handled)
            return;

        var eggs = PredictedSpawnAtPosition(ent.Comp.EggProto, Transform(ent).Coordinates);

        _audio.PlayPredicted(ent.Comp.LayEggSound, eggs, null);

        var selfMessage = Loc.GetString("spider-layegg-done-self");
        var othersMessage = Loc.GetString("spider-layegg-done-others", ("spider", ent.Owner));

        _popup.PopupPredicted(
            selfMessage,
            othersMessage,
            ent,
            ent,
            PopupType.LargeCaution);

        args.Handled = true;

    }
}
