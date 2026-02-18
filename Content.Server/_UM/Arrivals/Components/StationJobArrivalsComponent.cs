using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._UM.Arrivals.Components;

/// <summary>
/// This is used for handling the tider shuttle, the rounstart vessel that delivers tiders
/// </summary>
[RegisterComponent]
public sealed partial class StationJobArrivalsComponent : Component
{
    [DataField]
    public ResPath ShuttlePath = new("/Maps/_UM/PassengerArrivals.yml");

    [DataField]
    public EntityUid? ShuttleUid;

    //Is this shuttle docke already?
    [DataField]
    public bool Docked = false;

    [DataField]
    public ProtoId<JobPrototype> Job = "Passenger";
}
