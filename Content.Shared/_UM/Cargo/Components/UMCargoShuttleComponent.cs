using Content.Shared.Cargo;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._UM.Cargo.Components;

/// <summary>
/// Added to the cargo shuttle
/// </summary>
[RegisterComponent]
public sealed partial class UMCargoShuttleComponent : Component
{
    [DataField]
    public List<CargoOrderData> CurrentOrders = new();

    /// <summary>
    /// if we still need to do the initial warp
    /// (warp to station while in station space)
    /// </summary>
    [DataField]
    public bool FirstWarp = true;

    public bool DumpMobs = true;

    /// <summary>
    ///     The paper-type prototype to spawn with the order information.
    /// </summary>
    [DataField("printerOutput", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>)), ViewVariables(VVAccess.ReadWrite)]
    public string PrinterOutput = "PaperCargoInvoice";
}
