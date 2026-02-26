using Robust.Shared.GameStates;

namespace Content.Shared._UM.Audio.AmbientMusic;

/// <summary>
///     Marks an entity as contributing to Engineering ambient music.
///     Ported from Ephemeral Space
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class UMAmbientMusicMarkerEngineeringComponent : Component;
