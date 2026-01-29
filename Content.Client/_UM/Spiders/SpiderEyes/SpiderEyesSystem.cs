using Content.Shared._UM.Spiders.SpiderEyes;
using Robust.Client.GameObjects;
using Robust.Shared.Player;

namespace Content.Client._UM.Spiders.SpiderEyes;

/// <summary>
/// This handles...
/// </summary>
public sealed class SpiderEyesSystem : SharedSpiderEyesSystem
{
    [Dependency] private readonly PointLightSystem _pointLightSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpiderEyesComponent, ToggleSpiderEyesActionEvent>(OnToggleEyes);
        SubscribeLocalEvent<SpiderEyesComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<SpiderEyesComponent, PlayerDetachedEvent>(OnDetach);
    }

    private void OnToggleEyes(Entity<SpiderEyesComponent> ent, ref ToggleSpiderEyesActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<PointLightComponent>(ent, out var light))
            return;

        if (light.Enabled)
        {
            _pointLightSystem.SetEnabled(ent.Owner, false, light);
            return;
        }

        _pointLightSystem.SetEnabled(ent.Owner, true, light);

        args.Handled = true;
    }

    private void OnRemove(Entity<SpiderEyesComponent> ent, ref ComponentRemove args)
    {
        if (!TryComp<PointLightComponent>(ent, out var light))
            return;

        _pointLightSystem.SetEnabled(ent, false, light);
    }

    private void OnDetach(Entity<SpiderEyesComponent> ent, ref PlayerDetachedEvent args)
    {
        if (!TryComp<PointLightComponent>(ent, out var light))
            return;

        _pointLightSystem.SetEnabled(ent, false, light);
    }
}
