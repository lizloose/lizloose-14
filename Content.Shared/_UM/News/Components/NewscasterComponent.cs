using Content.Shared.MassMedia.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared._UM.News.Components;

/// <summary>
/// This is used for handling news reading machines
/// </summary>
[RegisterComponent]
public sealed partial class NewscasterComponent : Component
{
}



[Serializable, NetSerializable]
public enum NewscasterUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class NewscasterBoundUserInterfaceState : BoundUserInterfaceState
{
    public List<NewsArticle> Articles;

    public NewscasterBoundUserInterfaceState(List<NewsArticle> articles)
    {
        Articles = articles;
    }
}
