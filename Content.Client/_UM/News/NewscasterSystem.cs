using Content.Shared._UM.News;
using Content.Shared._UM.News.Components;

namespace Content.Client._UM.News;

/// <summary>
/// This handles...
/// </summary>
public sealed class NewscasterSystem : SharedNewscasterSystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NewscasterComponent, AfterAutoHandleStateEvent>(OnNewscasterState);
    }

    private void OnNewscasterState(Entity<NewscasterComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateUi(ent);
    }

    protected override void UpdateUi(Entity<NewscasterComponent> ent)
    {
        if (UserInterfaceSystem.TryGetOpenUi(ent.Owner, NewscasterUiKey.Key, out var bui))
        {
            bui.Update();
        }
    }
}
