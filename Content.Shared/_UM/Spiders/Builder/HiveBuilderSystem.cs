using System.Linq;
using Content.Shared._UM.Energy;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Physics;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Spiders.Builder;

public sealed class HiveBuilderSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly EnergyContainerSystem _energy = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private const string HiveBuilderBuiXmlGeneratedName = "HiveBuilderSelectTypeBoundUserInterface";
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HiveBuilderComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<HiveBuilderComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<HiveBuilderComponent, HiveBuilderSelectTypeActionEvent>(OnSelectTypeAction);
        SubscribeLocalEvent<HiveBuilderComponent, HiveBuilderTypeSelectMessage>(OnSelectTypeMessage);

        SubscribeLocalEvent<HiveBuilderComponent, HiveBuilderBuildActionEvent>(OnBuildAction);
        SubscribeLocalEvent<HiveBuilderComponent, OnBuildDoAfterEvent>(OnBuildDoAfter);
    }

    private void OnMapInit(Entity<HiveBuilderComponent> ent, ref MapInitEvent args)
    {
        _actionsSystem.AddAction(ent, ref ent.Comp.RadialActionEntity, ent.Comp.RadialAction);
        _actionsSystem.AddAction(ent, ref ent.Comp.BuildActionEntity, ent.Comp.BuildAction);

        var userInterfaceComp = EnsureComp<UserInterfaceComponent>(ent);
        _uiSystem.SetUi((ent, userInterfaceComp), HiveBuilderRadialUiKey.Key, new InterfaceData(HiveBuilderBuiXmlGeneratedName));

        ent.Comp.CurrentBuild = ent.Comp.BuildTypes.First();
        Dirty(ent);
    }

    private void OnShutdown(Entity<HiveBuilderComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.RadialActionEntity != null)
        {
            _actionsSystem.RemoveAction(ent.Owner, ent.Comp.RadialActionEntity);
        }
        if (ent.Comp.BuildActionEntity != null)
        {
            _actionsSystem.RemoveAction(ent.Owner, ent.Comp.BuildActionEntity);
        }
    }

    private void OnSelectTypeAction(Entity<HiveBuilderComponent> ent, ref HiveBuilderSelectTypeActionEvent args)
    {
        if (!TryComp<UserInterfaceComponent>(ent, out var userInterfaceComp))
            return;

        _uiSystem.OpenUi((ent, userInterfaceComp), HiveBuilderRadialUiKey.Key, args.Performer);
    }

    private void OnSelectTypeMessage(Entity<HiveBuilderComponent> ent, ref HiveBuilderTypeSelectMessage args)
    {
        if (!ent.Comp.BuildTypes.Contains(args.PrototypeId))
            return;

        ent.Comp.CurrentBuild = args.PrototypeId;
    }

    private void OnBuildAction(Entity<HiveBuilderComponent> ent, ref HiveBuilderBuildActionEvent args)
    {
        if (!_prototypeManager.TryIndex(ent.Comp.CurrentBuild, out var buildPrototype))
            return;

        if (!_energy.CanSpendEnergy(ent.Owner, ent.Comp.EnergyName, buildPrototype.Cost))
            return;

        var xform = Transform(ent);

        if (!_transform.InRange(xform.Coordinates, args.Target, SharedInteractionSystem.InteractionRange))
            return;

        var gridUid = _transform.GetGrid(args.Target);
        if (!TryComp<MapGridComponent>(gridUid, out var mapGrid))
            return;

        var position = _mapSystem.TileIndicesFor(gridUid.Value, mapGrid, args.Target);

        var anchored = _mapSystem.GetAnchoredEntities((gridUid.Value, mapGrid), position);

        var physQuery = GetEntityQuery<PhysicsComponent>();
        foreach (var entity in anchored)
        {
            var proto = Prototype(entity);
            if (proto != null && proto == buildPrototype.Prototype)
                return;
            if (!physQuery.TryGetComponent(entity, out var body))
                continue;
            if (body.BodyType != BodyType.Static ||
                !body.Hard ||
                (body.CollisionLayer & (int) CollisionGroup.MidImpassable) == 0)
                continue;

            return;
        }

        var doafterTarget = _mapSystem.GridTileToLocal(gridUid.Value, mapGrid, position);

        var doAfter = new OnBuildDoAfterEvent()
        {
            TargetCoordinates = GetNetCoordinates(doafterTarget),
        };

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, buildPrototype.Delay, doAfter, ent)
        {
            BreakOnMove = true,
            NeedHand = false,
        });
    }

    private void OnBuildDoAfter(Entity<HiveBuilderComponent> ent, ref OnBuildDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!_prototypeManager.TryIndex(ent.Comp.CurrentBuild, out var buildPrototype))
            return;

        if (!_energy.TrySpendEnergy(ent.Owner, ent.Comp.EnergyName, buildPrototype.Cost))
            return;

        PredictedSpawnAtPosition(buildPrototype.Prototype, GetCoordinates(args.TargetCoordinates));

        args.Handled = true;
    }
}
