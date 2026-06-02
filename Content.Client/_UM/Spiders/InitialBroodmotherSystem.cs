using Content.Shared._UM.Spiders;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client._UM.Spiders;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class InitialBroodmotherSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototype = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<UMInitialBroodmotherComponent, GetStatusIconsEvent>(GetBroodmotherIcon);
    }

    private void GetBroodmotherIcon(Entity<UMInitialBroodmotherComponent> ent, ref GetStatusIconsEvent args)
    {
        var iconPrototype = _prototype.Index(ent.Comp.StatusIcon);
        args.StatusIcons.Add(iconPrototype);
    }
}
