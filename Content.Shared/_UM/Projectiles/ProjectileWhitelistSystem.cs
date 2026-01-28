using Content.Shared.Damage;
using Content.Shared.Projectiles;
using Content.Shared.Whitelist;

namespace Content.Shared._UM.Projectiles;

public sealed class ProjectileWhitelistSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ProjectileWhitelistComponent, ProjectileHitEvent>(OnHit);
    }

    private void OnHit(Entity<ProjectileWhitelistComponent> ent, ref ProjectileHitEvent args)
    {
        if (_whitelist.IsWhitelistFail(ent.Comp.Whitelist, args.Target))
            return;

        if (ent.Comp.WhitelistDamage == null)
        {
            args.Damage = new DamageSpecifier();
            return;
        }

        args.Damage = ent.Comp.WhitelistDamage;
    }
}
