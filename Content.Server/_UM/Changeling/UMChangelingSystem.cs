using Content.Server.Store.Systems;
using Content.Shared._UM.Changeling;
using Content.Shared.FixedPoint;

namespace Content.Server._UM.Changeling;

/// <summary>
/// This handles...
/// </summary>
public sealed class UMChangelingSystem : UMSharedChangelingSystem
{
    [Dependency] private readonly StoreSystem _store = default!;

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
