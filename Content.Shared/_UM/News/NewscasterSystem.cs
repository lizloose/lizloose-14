using System.Diagnostics.CodeAnalysis;
using Content.Shared._UM.News.Components;
using Content.Shared.Chat;
using Content.Shared.MassMedia.Components;
using Content.Shared.MassMedia.Systems;
using Content.Shared.Station;

namespace Content.Shared._UM.News;

/// <summary>
/// This handles news reading machines, such as the newscaster
/// </summary>
public sealed class NewscasterSystem : EntitySystem
{
    [Dependency] private readonly SharedStationSystem _station = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NewscasterComponent, BoundUIOpenedEvent>(OnOpened);
    }

    private void OnOpened(Entity<NewscasterComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateNewscasterUiState(ent);
    }

    /// <summary>
    /// Updates a newscaster with a new article, makes it send the headline as a chat message
    /// </summary>
    public void OnNewArticle(Entity<NewscasterComponent> ent, NewsArticle article)
    {
        _chat.TrySendInGameICMessage(ent.Owner, article.Title, InGameICChatType.Speak, hideChat: true);
        UpdateNewscasterUiState(ent);
    }

    /// <summary>
    /// Update the UI state for a newscaster entity
    /// </summary>
    public void UpdateNewscasterUiState(Entity<NewscasterComponent> ent)
    {
        if (!TryGetArticles(ent.Owner, out var articles))
            return;

        var state = new NewscasterBoundUserInterfaceState(articles);
        _ui.SetUiState(ent.Owner, NewscasterUiKey.Key, state);
    }

    private bool TryGetArticles(EntityUid uid, [NotNullWhen(true)] out List<NewsArticle>? articles)
    {
        if (_station.GetOwningStation(uid) is not { } station ||
            !TryComp<StationNewsComponent>(station, out var stationNews))
        {
            articles = null;
            return false;
        }

        articles = stationNews.Articles;
        return true;
    }
}
