using Content.Shared._UM.Energy;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
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
    [Dependency] private readonly EnergyContainerSystem _energy = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    private const string SpiderEvolveBuiXmlGeneratedName = "SpiderEvolutionBoundUserInterface";
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpiderEvolutionComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SpiderEvolutionComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SpiderEvolutionComponent, SpiderEvolveActionEvent>(OnEvolveAction);
        SubscribeLocalEvent<SpiderEvolutionComponent, SpiderEvolutionSelectMessage>(OnEvolutionSelect);
        SubscribeLocalEvent<SpiderEvolutionComponent, EvolveDoAfterEvent>(OnEvolutionDoAfter);
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
        if (!_energy.CanSpendEnergy(ent.Owner, ent.Comp.EnergyName, ent.Comp.EvolutionCost))
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

        if (!_energy.CanSpendEnergy(ent.Owner, ent.Comp.EnergyName, ent.Comp.EvolutionCost))
            return;

        var doAfter = new EvolveDoAfterEvent()
        {
            ProtoId = args.PrototypeId,
        };

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, ent.Comp.EvolutionDuration, doAfter, ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
        });

        var selfMessage = Loc.GetString("spider-evolve-start-self");
        var othersMessage = Loc.GetString("spider-evolve-start-others", ("spider", ent.Owner));

        _popup.PopupPredicted(
            selfMessage,
            othersMessage,
            ent,
            ent,
            PopupType.MediumCaution);
    }

    private void OnEvolutionDoAfter(Entity<SpiderEvolutionComponent> ent, ref EvolveDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!ent.Comp.EvolutionTypes.Contains(args.ProtoId))
            return;

        if (_net.IsClient)
            return;

        if (!_energy.CanSpendEnergy(ent.Owner, ent.Comp.EnergyName, ent.Comp.EvolutionCost))
            return;

        Evolve(ent, args.ProtoId);
        Del(ent);
    }

    public void Evolve(EntityUid ent, EntProtoId proto, bool deleteOriginal = false)
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
        if (deleteOriginal)
            Del(ent);
    }
}
