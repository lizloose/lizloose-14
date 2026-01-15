using Content.Server._UM.Antags.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared._UM.Antags.Victim.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles;
using Robust.Shared.Random;

namespace Content.Server._UM.Antags.Objectives.Systems;

/// <inheritdoc/>
public sealed class DamageSelfObjectiveSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;
    [Dependency] private readonly CodeConditionSystem _codeCondition = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VictimComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<DamageSelfObjectiveComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<DamageSelfObjectiveComponent, ObjectiveGetProgressEvent>(OnGetProgress);
        SubscribeLocalEvent<DamageSelfObjectiveComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
    }

    private void OnMapInit(Entity<DamageSelfObjectiveComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.Damage = _random.Next(ent.Comp.MinDamage, ent.Comp.MaxDamage);
        Log.Debug("This is the number: " + ent.Comp.Damage);
    }

    private void OnGetProgress(Entity<DamageSelfObjectiveComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        if (ent.Comp.DamageDealt > ent.Comp.Damage)
            args.Progress = 1f;
        else
            args.Progress = (ent.Comp.DamageDealt / ent.Comp.Damage).Float();
    }

    private void OnAfterAssign(Entity<DamageSelfObjectiveComponent> ent, ref ObjectiveAfterAssignEvent args)
    {
        var title = Loc.GetString("objective-condition-deal-damage-to-self-title", ("amount", ent.Comp.Damage));

        _metaData.SetEntityName(ent, title, args.Meta);
    }

    private void OnDamageChanged(Entity<VictimComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased || ent.Owner != args.Origin)
            return;

        if (args.DamageDelta == null)
            return;

        if (!_mind.TryGetMind(ent, out var mind, out _))
            return;

        if (!_roles.MindHasRole<VictimRoleComponent>(mind))
            return;

        if (!_mind.TryGetObjectiveComp<DamageSelfObjectiveComponent>(ent, out var obj))
            return;

        obj.DamageDealt += args.DamageDelta.GetTotal();

        if (obj.DamageDealt > obj.Damage)
            _codeCondition.SetCompleted(ent.Owner, "DamageSelfObjective");
    }
}
