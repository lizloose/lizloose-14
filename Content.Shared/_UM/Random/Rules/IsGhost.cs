using Content.Shared.Ghost;
using Content.Shared.Random.Rules;

namespace Content.Shared._UM.Random.Rules;

/// <summary>
/// Returns true if the attached entity is a ghost
/// </summary>
public sealed partial class IsGhostRule : RulesRule
{
    public override bool Check(EntityManager entManager, EntityUid uid)
    {
        if (!entManager.TryGetComponent(uid, out GhostComponent? ghost))
        {
            return Inverted;
        }

        return !Inverted;
    }
}
