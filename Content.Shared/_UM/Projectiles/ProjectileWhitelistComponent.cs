using Content.Shared.Damage;
using Content.Shared.Whitelist;

namespace Content.Shared._UM.Projectiles;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class ProjectileWhitelistComponent : Component
{
    /// <summary>
    /// Projectiles that hit something in this whitelist will either do no damage or the damage in WhitelistDamge
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist Whitelist = new();

    /// <summary>
    /// Replacement damage to deal
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier? WhitelistDamage;
}
