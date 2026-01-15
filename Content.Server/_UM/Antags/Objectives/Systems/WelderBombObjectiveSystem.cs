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

public sealed class WelderBombObjectiveSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly CodeConditionSystem _codeCondition = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VictimComponent, WelderBombEvent>(OnWelderBomb);
        SubscribeLocalEvent<WelderBombObjectiveComponent, RequirementCheckEvent>(OnWelderBombRequirementCheck);
        SubscribeLocalEvent<WelderBombObjectiveComponent, ObjectiveGetProgressEvent>(OnWelderBombGetProgress);
        SubscribeLocalEvent<WelderBombObjectiveComponent, ObjectiveAfterAssignEvent>(OnWelderBombAfterAssign);
    }

    private void OnWelderBombGetProgress(Entity<WelderBombObjectiveComponent> ent, ref ObjectiveGetProgressEvent args)
    {
    }

    private void OnWelderBombAfterAssign(Entity<WelderBombObjectiveComponent> ent, ref ObjectiveAfterAssignEvent args)
    {
        if (!TryComp<WarpPointComponent>(ent.Comp.Target, out var warp) || warp.Location == null)
            return;

        var title = Loc.GetString("objective-condition-welder-tank-title", ("location", warp.Location));

        _metaData.SetEntityName(ent, title, args.Meta);
    }

    private void OnWelderBombRequirementCheck(Entity<WelderBombObjectiveComponent> ent, ref RequirementCheckEvent args)
    {
        if (args.Cancelled)
            return;

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
            args.Cancelled = true;
            return;
        }
        ent.Comp.Target = _random.Pick(warps);
    }

    private void OnWelderBomb(Entity<VictimComponent> ent, ref WelderBombEvent args)
    {
        Log.Debug("welder bombed");

        if (!_mind.TryGetMind(ent, out var mind, out _))
            return;

        if (!_roles.MindHasRole<VictimRoleComponent>(mind))
            return;

        if (!_mind.TryGetObjectiveComp<WelderBombObjectiveComponent>(ent, out var obj) || obj.Target == null)
            return;

        var tankXform = Transform(args.Tank);
        var targetXform = Transform(obj.Target.Value);

        if (tankXform.MapID != targetXform.MapID || (_transform.GetWorldPosition(tankXform) - _transform.GetWorldPosition(targetXform)).LengthSquared() > obj.Range * obj.Range)
            return;

        Log.Debug("welder bomb obj:" + nameof(obj));
        if (ent.Comp.BombEnabled)
            _codeCondition.SetCompleted(ent.Owner, "WelderBombObjective");
    }
}
