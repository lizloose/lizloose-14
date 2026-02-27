using System.Diagnostics.CodeAnalysis;
using Content.Shared._UM.News.Components;
using Content.Shared.Chat;
using Content.Shared.MassMedia.Components;
using Content.Shared.MassMedia.Systems;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Station;

namespace Content.Shared._UM.News;

/// <summary>
/// This handles news reading machines, such as the newscaster
/// </summary>
public abstract class SharedNewscasterSystem : EntitySystem
{
    [Dependency] private readonly SharedStationSystem _station = default!;
    [Dependency] protected readonly SharedUserInterfaceSystem UserInterfaceSystem = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _powerReceiver = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NewscasterComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<NewscasterComponent> ent, ref MapInitEvent args)
    {
        UpdateNewscaster(ent);
    }

    /// <summary>
    /// Updates a newscaster with a new article, makes it send the headline as a chat message
    /// </summary>
    public void OnNewArticle(Entity<NewscasterComponent> ent, NewsArticle article)
    {
        UpdateNewscaster(ent);

        if (_powerReceiver.IsPowered(ent.Owner))
            _chat.TrySendInGameICMessage(ent.Owner, article.Title, InGameICChatType.Speak, hideChat: true);
    }

    /// <summary>
    /// Update the UI state for a newscaster entity
    /// </summary>
    public void UpdateNewscaster(Entity<NewscasterComponent> ent)
    {
        if (!TryGetArticles(ent.Owner, out var articles))
            return;

        ent.Comp.Articles = articles;
        Dirty(ent);
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

    protected virtual void UpdateUi(Entity<NewscasterComponent> ent)
    {
    }
}
