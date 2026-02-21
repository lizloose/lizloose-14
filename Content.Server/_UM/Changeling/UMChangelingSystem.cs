using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds;
using Content.Server.Destructible.Thresholds.Behaviors;
using Content.Server.Store.Systems;
using Content.Shared._UM.Changeling;
using Content.Shared._UM.Changeling.Components;
using Content.Shared.FixedPoint;

namespace Content.Server._UM.Changeling;

/// <summary>
/// This handles...
/// </summary>
public sealed class UMChangelingSystem : UMSharedChangelingSystem
{
    [Dependency] private readonly StoreSystem _store = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<UMChangelingComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<UMChangelingComponent> ent, ref MapInitEvent args)
    {
        //Cheap hack to make gibbing changelings impossible.
        //For now.
        RemComp<DestructibleComponent>(ent.Owner);
    }

    public override bool TryAddStorePoints(EntityUid ent, FixedPoint2 points)
    {
        if (!_store.TryAddCurrency(new Dictionary<string, FixedPoint2>
                {
                    { "ChangelingDNA", points }
                },
                ent))
            return false;

        return true;
    }
}
