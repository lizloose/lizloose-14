using System.Linq;
using Content.Server._UM.Shuttle;
using Content.Server.Cargo.Components;
using Content.Server.Cargo.Systems;
using Content.Server.Popups;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared._UM.Cargo.Components;
using Content.Shared.Cargo.Components;
using Content.Shared.Cargo.Prototypes;
using Content.Shared.CCVar;
using Content.Shared.Station;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._UM.Cargo;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class UMCargoSystem : EntitySystem
{
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly SharedStationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly CargoSystem _cargoSystem = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    // ReSharper disable once InconsistentNaming
    [Dependency] private readonly UMShuttleSystem _UMShuttle = default!;

    private bool _lockboxCutEnabled;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        InitializeConsole();
        InitializeShuttle();

        SubscribeLocalEvent<FulfillCargoOrderEvent>(OnFulfillCargoOrder);
        _cfg.OnValueChanged(CCVars.LockboxCutEnabled, (enabled) => { _lockboxCutEnabled = enabled; }, true);
    }

    private void OnFulfillCargoOrder(ref FulfillCargoOrderEvent args)
    {
        var query = EntityQueryEnumerator<UMCargoShuttleComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var shuttle, out var shuttleXform))
        {
            if (_station.GetOwningStation(uid) != args.Station)
                continue;

            for (var i = 0; i < args.Order.OrderQuantity; i++)
            {
                shuttle.CurrentOrders.Add(args.Order);
            }

            args.Handled = true;
            args.FulfillmentEntity = uid;

            if (!TryComp<StationCentcommComponent>(args.Station, out var centcomm) || centcomm.MapEntity == null)
                return;
            if (shuttleXform.MapUid == centcomm.MapEntity)
                TryFulfillOrders((uid, shuttle));
            return;
        }
    }

    private void TryFulfillOrders(Entity<UMCargoShuttleComponent> ent)
    {
        var shuttleXform = Transform(ent);
        if (shuttleXform.GridUid == null)
            return;

        var query = AllEntityQuery<CargoPalletComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out _, out var xform))
        {
            if (xform.GridUid != shuttleXform.GridUid)
                continue;

            if (ent.Comp.CurrentOrders.Count == 0)
                return;

            var currentOrder = ent.Comp.CurrentOrders.First();
            if (_cargoSystem.FulfillOrder(currentOrder, currentOrder.Account, xform.Coordinates, ent.Comp.PrinterOutput))
            {
                if (_station.GetOwningStation(uid) is { } station)
                    _cargoSystem.UpdateOrders(station);

                ent.Comp.CurrentOrders.Remove(currentOrder);
            }
        }
    }

    private void PalletSell(Entity<UMCargoShuttleComponent> ent)
    {
        var xform = Transform(ent);

        if (_station.GetOwningStation(ent) is not { } station ||
            !TryComp<StationBankAccountComponent>(station, out var bankAccount))
        {
            return;
        }

        if (xform.GridUid is not { } gridUid)
            return;

        if (!_cargoSystem.SellPallets(gridUid, station, out var goods))
            return;

        var baseDistribution = _cargoSystem.CreateAccountDistribution((station, bankAccount));
        foreach (var (_, sellComponent, value) in goods)
        {
            Dictionary<ProtoId<CargoAccountPrototype>, double> distribution;
            if (sellComponent != null)
            {
                var cut = _lockboxCutEnabled ? bankAccount.LockboxCut : bankAccount.PrimaryCut;
                distribution = new Dictionary<ProtoId<CargoAccountPrototype>, double>
                {
                    { sellComponent.OverrideAccount, cut },
                    { bankAccount.PrimaryAccount, 1.0 - cut },
                };
            }
            else
            {
                distribution = baseDistribution;
            }

            _cargoSystem.UpdateBankAccount((station, bankAccount), (int) Math.Round(value), distribution, false);
        }

        Dirty(station, bankAccount);
    }
}
