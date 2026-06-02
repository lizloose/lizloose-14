using Content.Server._UM.Antags.Objectives.Components;
using Content.Server.Damage.Components;
using Content.Server.Objectives.Systems;
using Content.Shared._UM.Antags.Victim.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles;
using Content.Shared.Warps;
using Content.Shared.Whitelist;
using Robust.Shared.Random;

namespace Content.Server._UM.Antags.Objectives.Systems;

public sealed partial class WelderBombObjectiveSystem : EntitySystem
{
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private SharedRoleSystem _roles = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private MetaDataSystem _metaData = default!;
    [Dependency] private CodeConditionSystem _codeCondition = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VictimComponent, WelderBombEvent>(OnWelderBomb);
        SubscribeLocalEvent<WelderBombObjectiveComponent, RequirementCheckEvent>(OnWelderBombRequirementCheck);
        SubscribeLocalEvent<WelderBombObjectiveComponent, ObjectiveAfterAssignEvent>(OnWelderBombAfterAssign);
    }

    private void OnWelderBombAfterAssign(Entity<WelderBombObjectiveComponent> ent, ref ObjectiveAfterAssignEvent args)
    {
        if (!TryComp<WarpPointComponent>(ent.Comp.Target, out var warp) || warp.Location == null)
        {
            var title = Loc.GetString("objective-condition-welder-tank-title-no-location");
            _metaData.SetEntityName(ent, title, args.Meta);
            return;
        }

        var titleLocation = Loc.GetString("objective-condition-welder-tank-title", ("location", warp.Location));

        _metaData.SetEntityName(ent, titleLocation, args.Meta);
    }

    private void OnWelderBombRequirementCheck(Entity<WelderBombObjectiveComponent> ent, ref RequirementCheckEvent args)
    {
        if (args.Cancelled)
            return;

        foreach (var obj in args.Mind.Objectives)
        {
            if (HasComp<WelderBombObjectiveComponent>(obj))
            {
                args.Cancelled = true;
                return;
            }
        }

        //TODO Make this a generic location objective system
        if (ent.Comp.NoLocation)
        {
            ent.Comp.Target = null;
            return;
        }

        var warps = new List<EntityUid>();
        var allEnts = EntityQueryEnumerator<WarpPointComponent>();
        var bombingBlacklist = ent.Comp.Blacklist;

        while (allEnts.MoveNext(out var warpUid, out var warp))
        {
            if (_whitelist.IsWhitelistFail(bombingBlacklist, warpUid)
                && !string.IsNullOrWhiteSpace(warp.Location))
            {
                warps.Add(warpUid);
            }
        }

        if (warps.Count <= 0)
        {
            ent.Comp.Target = null;
            return;
        }
        ent.Comp.Target = _random.Pick(warps);
    }

    private void OnWelderBomb(Entity<VictimComponent> ent, ref WelderBombEvent args)
    {
        if (!_mind.TryGetMind(ent, out var mind, out _))
            return;

        if (!_roles.MindHasRole<VictimRoleComponent>(mind))
            return;

        if (!_mind.TryGetObjectiveComp<WelderBombObjectiveComponent>(ent, out var obj))
            return;

        if (obj.Target == null) //if it's null we give them the win anyways, they blew up the bomb.
        {
            _codeCondition.SetCompleted(ent.Owner, "WelderBombObjective");
            return;
        }

        var tankXform = Transform(args.Tank);
        var targetXform = Transform(obj.Target.Value);

        if (tankXform.MapID != targetXform.MapID || (_transform.GetWorldPosition(tankXform) - _transform.GetWorldPosition(targetXform)).LengthSquared() > obj.Range * obj.Range)
            return;

        if (ent.Comp.BombEnabled)
            _codeCondition.SetCompleted(ent.Owner, "WelderBombObjective");
    }
}
