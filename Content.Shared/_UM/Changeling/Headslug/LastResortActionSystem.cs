using System.Linq;
using Content.Shared._UM.Changeling.Headslug.Components;
using Content.Shared.Body;
using Content.Shared.Explosion.EntitySystems;
using Content.Shared.Gibbing;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;

namespace Content.Shared._UM.Changeling.Headslug;

/// <summary>
/// This handles...
/// </summary>
public sealed class LastResortActionSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly GibbingSystem _gibbing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LastResortActionComponent, LastResortActionEvent>(OnLastResortAction);
    }

    private void OnLastResortAction(Entity<LastResortActionComponent> ent, ref LastResortActionEvent args)
    {
        if (args.Handled)
            return;

        if (_net.IsClient)
            return;

        if (!_mind.TryGetMind(args.Performer, out var mind, out _))
            return;

        _gibbing.Gib(args.Performer);

        var xform = Transform(args.Performer);
        var slug = PredictedSpawnAtPosition(ent.Comp.ProtoId, xform.Coordinates);

        _mind.TransferTo(mind, slug);
        _mind.UnVisit(mind);

        args.Handled = true;
    }
}
