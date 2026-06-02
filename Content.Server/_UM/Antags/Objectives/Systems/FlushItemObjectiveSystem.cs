using Content.Server._UM.Antags.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared._UM.Antags.Victim.Components;
using Content.Shared.Disposal.Components;
using Content.Shared.Disposal.Unit.Events;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._UM.Antags.Objectives.Systems;

/// <summary>
/// This is probably shitcode. Low confidence here.
/// </summary>
public sealed partial class FlushItemObjectiveSystem : EntitySystem
{
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private SharedRoleSystem _roles = default!;
    [Dependency] private CodeConditionSystem _codeCondition = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private MetaDataSystem _metaData = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VictimComponent, OnDisposalInsertEvent>(OnDisposalInsert);
        SubscribeLocalEvent<FlushItemObjectiveBinMarkerComponent, BeforeDisposalFlushEvent>(OnDisposalFlush);
        SubscribeLocalEvent<FlushItemObjectiveComponent, ObjectiveGetProgressEvent>(OnGetProgress);

        SubscribeLocalEvent<FlushItemObjectiveComponent, RequirementCheckEvent>(OnRequirementCheck);
        SubscribeLocalEvent<FlushItemObjectiveComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
    }

    private void OnGetProgress(Entity<FlushItemObjectiveComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        if (ent.Comp.ItemInserted)
            args.Progress = 0.5f;

        if (ent.Comp.ItemInserted && ent.Comp.BinFlushed)
            args.Progress = 1.0f;
    }

    private void OnRequirementCheck(Entity<FlushItemObjectiveComponent> ent, ref RequirementCheckEvent args)
    {
        var picked = false;
        while (picked == false)
        {
            var item = _random.Pick(ent.Comp.Targets);
            if (!_prototypeManager.TryIndex<EntityPrototype>(item, out var itemPrototype))
                continue;

            ent.Comp.Target =  itemPrototype;
            picked = true;
        }
    }

    private void OnAfterAssign(Entity<FlushItemObjectiveComponent> ent, ref ObjectiveAfterAssignEvent args)
    {
        var title = Loc.GetString("objective-condition-steal-and-flush-title", ("item", ent.Comp.Target.Name));
        _metaData.SetEntityName(ent, title, args.Meta);
    }

    private void OnDisposalInsert(Entity<VictimComponent> ent, ref OnDisposalInsertEvent args)
    {
        if (!_mind.TryGetMind(ent, out var mind, out _))
            return;

        if (!_roles.MindHasRole<VictimRoleComponent>(mind))
            return;

        if (!_mind.TryGetObjectiveComp<FlushItemObjectiveComponent>(ent, out var obj))
            return;

        if (obj.ItemInserted && obj.BinFlushed)
            return;

        //TODO: Use generic target system when thats added
        // this is --good enough-- for playtesting until thats added
        if (Prototype(args.Inserted) != obj.Target)
            return;

        var markerComp = EnsureComp<FlushItemObjectiveBinMarkerComponent>(args.Target);

        markerComp.Item = args.Inserted;
        markerComp.ObjectiveOwner = ent.Owner;
        obj.ItemInserted = true;
    }

    private void OnDisposalFlush(Entity<FlushItemObjectiveBinMarkerComponent> ent, ref BeforeDisposalFlushEvent args)
    {
        if (!_mind.TryGetObjectiveComp<FlushItemObjectiveComponent>(ent.Comp.ObjectiveOwner, out var obj) || (obj.ItemInserted && obj.BinFlushed))
        {
            RemComp<FlushItemObjectiveBinMarkerComponent>(ent);
            return;
        }

        if (!_container.ContainsEntity(ent.Owner, ent.Comp.Item))
        {
            RemComp<FlushItemObjectiveBinMarkerComponent>(ent);
            obj.ItemInserted = false;
            return;
        }

        _codeCondition.SetCompleted(ent.Comp.ObjectiveOwner, "FlushItemObjective");
        QueueDel(ent.Comp.Item); //Maybe I shouldn't? I don't know
        RemComp<FlushItemObjectiveBinMarkerComponent>(ent);
        obj.BinFlushed = true;
    }
}
