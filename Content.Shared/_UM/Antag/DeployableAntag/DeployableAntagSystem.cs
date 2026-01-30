using Content.Shared._UM.Spiders.Evolution;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Gibbing;
using Content.Shared.Popups;
using Robust.Shared.Network;

namespace Content.Shared._UM.Antag.DeployableAntag;

/// <summary>
/// This handles...
/// </summary>
public sealed class DeployableAntagSystem : EntitySystem
{
    [Dependency] private readonly SpiderEvolutionSystem _evolution = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly GibbingSystem _gibbing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DeployableAntagComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<DeployableAntagComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<DeployableAntagComponent, DeployAntagActionEvent>(OnDeployAntagAction);
        SubscribeLocalEvent<DeployableAntagComponent, AntagDeployDoAfterEvent>(OnAntagDeployDoAfter);
    }

    private void OnMapInit(Entity<DeployableAntagComponent> ent, ref MapInitEvent args)
    {
        _actionsSystem.AddAction(ent, ref ent.Comp.DeployActionEntity, ent.Comp.DeployAction);
    }

    private void OnShutdown(Entity<DeployableAntagComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.DeployActionEntity != null)
        {
            _actionsSystem.RemoveAction(ent.Owner, ent.Comp.DeployActionEntity);
        }
    }

    private void OnDeployAntagAction(Entity<DeployableAntagComponent> ent, ref DeployAntagActionEvent args)
    {
        if (_net.IsClient)
            return;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, ent.Comp.DeployTime, new AntagDeployDoAfterEvent(), ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
        });
    }

    private void OnAntagDeployDoAfter(Entity<DeployableAntagComponent> ent, ref AntagDeployDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        var message = Loc.GetString(ent.Comp.DeployMessage, ("target", ent.Owner));

        _popupSystem.PopupClient(
            message,
            ent,
            ent,
            PopupType.LargeCaution);

        _evolution.Evolve(ent, ent.Comp.ProtoId);
        _gibbing.Gib(ent);

        args.Handled = true;
    }

}
