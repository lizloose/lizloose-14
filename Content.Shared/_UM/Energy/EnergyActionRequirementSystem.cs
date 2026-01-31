using Content.Shared.Actions.Events;

namespace Content.Shared._UM.Energy;

/// <summary>
/// This handles...
/// </summary>
public sealed class EnergyActionRequirementSystem : EntitySystem
{
    [Dependency] private readonly EnergyContainerSystem _energyContainer = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EnergyActionRequirementComponent, ActionAttemptEvent>(OnAttempt);
    }

    private void OnAttempt(Entity<EnergyActionRequirementComponent> ent, ref ActionAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!_energyContainer.TrySpendEnergy(args.User, ent.Comp.Name, ent.Comp.Amount))
        {
            args.Cancelled = true;
            return;
        }

    }
}
