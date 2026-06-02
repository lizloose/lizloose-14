using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._UM.Drip;

public sealed partial class DripTrackingSystem : EntitySystem
{
    [Dependency] private DripTrackingManager _tracking = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<PlayerDetachedEvent>(OnPlayerDetached);
    }

    private void OnPlayerAttached(PlayerAttachedEvent ev)
    {
        _tracking.QueueSendUpdate(ev.Player);
    }

    private void OnPlayerDetached(PlayerDetachedEvent ev)
    {
        _tracking.QueueSendUpdate(ev.Player);
    }

    public Dictionary<EntProtoId, int> GetAvailableItems(ICommonSession player)
    {
        var items = new Dictionary<EntProtoId, int>();

        if (!_tracking.TryGetTrackedDrip(player, out var availableItems))
            availableItems = new Dictionary<string, int>();

        foreach (var item in availableItems)
        {
            if (!_prototypes.TryIndex(item.Key, out var prototype))
                continue;

            items.Add(prototype, item.Value);
        }

        return items;
    }
}
