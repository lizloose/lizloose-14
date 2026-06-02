using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._UM.Spiders;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class UMInitialBroodmotherComponent : Component
{
    [DataField]
    public ProtoId<FactionIconPrototype> StatusIcon = "UMInitialBroodmotherFaction";
}
