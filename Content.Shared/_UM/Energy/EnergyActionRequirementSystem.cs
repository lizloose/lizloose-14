using Content.Shared._UM.Energy.Components;
using Content.Shared.Actions.Events;
using Content.Shared.Popups;

namespace Content.Shared._UM.Energy;

public sealed class EnergyActionRequirementSystem : EntitySystem
{
    [Dependency] private readonly EnergyContainerSystem _energyContainer = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EnergyActionRequirementComponent, ActionAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<EnergyActionRequirementComponent, ActionPerformedEvent>(OnPerform);
    }

    private void OnAttempt(Entity<EnergyActionRequirementComponent> ent, ref ActionAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (ent.Comp.OnlyCheck && _energyContainer.CanSpendEnergy(args.User, ent.Comp.EnergyName, ent.Comp.Amount))
            return;

        if (!_energyContainer.CanSpendEnergy(args.User, ent.Comp.EnergyName, ent.Comp.Amount))
        {
            var message = Loc.GetString(ent.Comp.CantFireMessage, ("energy", ent.Comp.FancyName));
            _popupSystem.PopupClient(message, args.User, args.User, PopupType.SmallCaution);
            args.Cancelled = true;
        }
    }

    private void OnPerform(Entity<EnergyActionRequirementComponent> ent, ref ActionPerformedEvent args)
    {
        if (!ent.Comp.OnlyCheck)
            _energyContainer.TrySpendEnergy(args.Performer, ent.Comp.EnergyName, ent.Comp.Amount);
    }

}
