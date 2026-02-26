using System.Numerics;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Shared.Mobs.Components;
using Robust.Shared.Map;

namespace Content.Server._UM.Shuttle;

/// <summary>
/// This handles...
/// </summary>
public sealed class UMShuttleSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;


    public void DumpChildren(EntityUid uid, ref FTLStartedEvent args)
    {
        var toDump = new List<Entity<TransformComponent>>();
        FindDumpChildren(uid, toDump);
        foreach (var (ent, xform) in toDump)
        {
            var rotation = xform.LocalRotation;
            _transform.SetCoordinates(ent, new EntityCoordinates(args.FromMapUid!.Value, Vector2.Transform(xform.LocalPosition, args.FTLFrom)));
            _transform.SetWorldRotation(ent, args.FromRotation + rotation);
        }
    }

    public bool HasDumpChildren(EntityUid uid)
    {
        var dumpable = new List<Entity<TransformComponent>>();
        FindDumpChildren(uid, dumpable);

        if (dumpable.Count == 0)
            return false;

        return true;
    }

    private void FindDumpChildren(EntityUid uid, List<Entity<TransformComponent>> toDump)
    {
        var xform = Transform(uid);

        if (HasComp<MobStateComponent>(uid) || HasComp<ArrivalsBlacklistComponent>(uid))
        {
            toDump.Add((uid, xform));
            return;
        }

        var children = xform.ChildEnumerator;
        while (children.MoveNext(out var child))
        {
            FindDumpChildren(child, toDump);
        }
    }
}
