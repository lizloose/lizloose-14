using Content.Shared.Ghost;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._UM.Ghost;

/// <summary>
/// A server to client response for a <see cref="GhostWarpsRequestEvent"/>.
/// Contains players, and locations a ghost can warp to
/// </summary>
[Serializable, NetSerializable]
public sealed class UMGhostWarpsResponseEvent : EntityEventArgs
{
    public UMGhostWarpsResponseEvent(List<UMPlayerWarp> playerWarps, List<UMLocationWarp> locationWarps)
    {
        PlayerWarps = playerWarps;
        LocationWarps = locationWarps;
    }

    public List<UMPlayerWarp> PlayerWarps { get; }

    public List<UMLocationWarp> LocationWarps { get; }
}


[Serializable, NetSerializable]
public struct UMPlayerWarp
{
    public UMPlayerWarp(NetEntity entity, string name, ProtoId<JobPrototype>? job, bool antagonist)
    {
        Entity = entity;
        Name = name;
        Job = job;
        Antagonist = antagonist;
    }

    public NetEntity Entity { get; }
    public string Name { get; }

    public ProtoId<JobPrototype>? Job = null;

    public bool Antagonist { get; }
}

[Serializable, NetSerializable]
public struct UMLocationWarp
{
    public UMLocationWarp(NetEntity entity, string name)
    {
        Entity = entity;
        Name = name;
    }

    public NetEntity Entity { get; }
    public string Name { get; }
}
