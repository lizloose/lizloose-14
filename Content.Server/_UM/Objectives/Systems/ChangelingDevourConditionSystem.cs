using Content.Server._UM.Objectives.Components;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared._UM.Changeling.Devour.Components;
using Content.Shared.Mind;

namespace Content.Server._UM.Objectives.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class ChangelingDevourConditionSystem : EntitySystem
{
    [Dependency] private readonly CodeConditionSystem _codeCondition = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChangelingDevouredEvent>(OnChangelingDevour);
    }

    private void OnChangelingDevour(ChangelingDevouredEvent ev)
    {
        //Handles counter objective
        var counterQuery = EntityQueryEnumerator<ChangelingDevourConditionComponent, GenericCounterComponent>();

        while (counterQuery.MoveNext(out _, out var counterObjComp))
        {
            counterObjComp.Count++;
        }

        //Handles target objectives
        if (!_mind.TryGetMind(ev.Target, out var mind, out _))
            return;

        var targetQuery = EntityQueryEnumerator<ChangelingDevourConditionComponent, TargetObjectiveComponent>();

        while (targetQuery.MoveNext(out var uid, out _, out var targetObjComp))
        {
            if (targetObjComp.Target == mind)
                _codeCondition.SetCompleted(uid);
        }
    }
}
