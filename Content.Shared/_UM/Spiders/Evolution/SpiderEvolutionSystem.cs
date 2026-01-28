using Content.Shared._UM.Spiders.SpiderEnergy;
using Content.Shared.Actions;
using Content.Shared.Emoting;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Spiders.Evolution;

public sealed class SpiderEvolutionSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SpiderEnergySystem _energy = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private const string SpiderEvolveBuiXmlGeneratedName = "SpiderEvolutionBoundUserInterface";
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpiderEvolutionComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SpiderEvolutionComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SpiderEvolutionComponent, SpiderEvolveActionEvent>(OnEvolveAction);
        SubscribeLocalEvent<SpiderEvolutionComponent, SpiderEvolutionSelectMessage>(OnEvolutionSelect);
    }

    private void OnMapInit(Entity<SpiderEvolutionComponent> ent, ref MapInitEvent args)
    {
        _actionsSystem.AddAction(ent, ref ent.Comp.RadialActionEntity, ent.Comp.RadialAction);
        var userInterfaceComp = EnsureComp<UserInterfaceComponent>(ent);

        _uiSystem.SetUi((ent, userInterfaceComp), SpiderEvolveRadialUiKey.Key, new InterfaceData(SpiderEvolveBuiXmlGeneratedName));
    }

    private void OnShutdown(Entity<SpiderEvolutionComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.RadialActionEntity != null)
        {
            _actionsSystem.RemoveAction(ent.Owner, ent.Comp.RadialActionEntity);
        }
    }
    private void OnEvolveAction(Entity<SpiderEvolutionComponent> ent, ref SpiderEvolveActionEvent args)
    {
        if (!_energy.CanSpendEnergy(ent.Owner, ent.Comp.EvolutionCost))
        {
            var message = Loc.GetString("spider-evolve-fail-energy");
            _popup.PopupClient(message, ent, PopupType.SmallCaution);
            return;
        }

        if (!TryComp<UserInterfaceComponent>(ent, out var userInterfaceComp))
            return;

        _uiSystem.OpenUi((ent, userInterfaceComp), SpiderEvolveRadialUiKey.Key, args.Performer);
    }

    private void OnEvolutionSelect(Entity<SpiderEvolutionComponent> ent, ref SpiderEvolutionSelectMessage args)
    {
        if (!ent.Comp.EvolutionTypes.Contains(args.PrototypeId))
            return;

        if (_net.IsClient)
            return;

        Evolve(ent, args.PrototypeId);
        Del(ent);
    }

    private void Evolve(Entity<SpiderEvolutionComponent> ent, EntProtoId proto)
    {
        if (_net.IsClient)
            return;

        if (!_mind.TryGetMind(ent, out var mind, out _))
            return;

        var coords = Transform(ent).Coordinates;
        var newSpider = PredictedSpawnAtPosition(proto, coords);
        _mind.TransferTo(mind, newSpider);
        _mind.UnVisit(mind);
        var oldRotation = _transform.GetWorldRotation(ent);
        _transform.SetWorldRotation(newSpider, oldRotation);
    }
}
