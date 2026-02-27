using Content.Shared.MassMedia.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._UM.News.Components;

/// <summary>
/// This is used for handling news reading machines
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class NewscasterComponent : Component
{
    /// <summary>
    /// List of articles this device currently has
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<NewsArticle> Articles = new();
}



[Serializable, NetSerializable]
public enum NewscasterUiKey : byte
{
    Key,
}
