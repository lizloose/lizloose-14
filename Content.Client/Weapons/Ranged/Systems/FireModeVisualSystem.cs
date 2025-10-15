using System.Linq;
using Content.Client.Clothing;
using Content.Client.Items.Systems;
using Content.Client.Weapons.Ranged.Components;
using Content.Shared.Clothing;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Wieldable.Components;
using Robust.Client.GameObjects;

namespace Content.Client.Weapons.Ranged.Systems;

/// <inheritdoc/>
public sealed class BatteryWeaponFireModesVisuals : EntitySystem
{
    [Dependency] private readonly SharedItemSystem _item = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<FireModeVisualsComponent, AppearanceChangeEvent>(OnAppearanceChange);
        SubscribeLocalEvent<FireModeVisualsComponent, GetInhandVisualsEvent>(OnGetHeldVisuals, after: [typeof(ItemSystem)]);
        SubscribeLocalEvent<FireModeVisualsComponent, GetEquipmentVisualsEvent>(OnGetEquipmentVisuals, after: [typeof(ClientClothingSystem)]);
    }

    private void OnAppearanceChange(Entity<FireModeVisualsComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        _item.VisualsChanged(ent);
    }

    private void OnGetHeldVisuals(Entity<FireModeVisualsComponent> ent, ref GetInhandVisualsEvent args)
    {
        if (!TryComp<BatteryWeaponFireModesComponent>(ent, out var fireModesComponent))
            return;

        if (!HasComp<AppearanceComponent>(ent))
            return;

        var color = fireModesComponent.FireModes[fireModesComponent.CurrentFireMode].Color;

        if (!ent.Comp.InhandVisuals.TryGetValue(args.Location, out var layers))
            return;

        var defaultKey = $"inhand-{args.Location.ToString().ToLowerInvariant()}-color";

        if (TryComp(ent, out WieldableComponent? wieldableComponent) && wieldableComponent.Wielded)
        {
            if (!ent.Comp.WieldedInhandVisuals.TryGetValue(args.Location, out var wieldedLayers))
                return;
            AddLayers(wieldedLayers, color, defaultKey, args);
            return;
        }
        AddLayers(layers, color, defaultKey, args);
    }

    private void AddLayers(List<PrototypeLayerData> layers, Color color, string defaultKey, GetInhandVisualsEvent args)
    {
        var i = 0;
        foreach (var layer in layers)
        {
            var key = layer.MapKeys?.FirstOrDefault();
            if (key == null)
            {
                key = i == 0 ? defaultKey : $"{defaultKey}-{i}";
                i++;
            }
            layer.Color =  color;
            args.Layers.Add((key, layer));
        }
    }

    private void OnGetEquipmentVisuals(Entity<FireModeVisualsComponent> ent, ref GetEquipmentVisualsEvent args)
    {
        if (!TryComp<BatteryWeaponFireModesComponent>(ent, out var fireModesComponent))
            return;

        if (!HasComp<AppearanceComponent>(ent))
            return;

        if (!TryComp(args.Equipee, out InventoryComponent? inventory))
            return;
        List<PrototypeLayerData>? layers = null;

        // attempt to get species specific data
        if (inventory.SpeciesId != null)
            ent.Comp.ClothingVisuals.TryGetValue($"{args.Slot}-{inventory.SpeciesId}", out layers);

        // No species specific data.  Try to default to generic data.
        if (layers == null && !ent.Comp.ClothingVisuals.TryGetValue(args.Slot, out layers))
            return;

        var color = fireModesComponent.FireModes[fireModesComponent.CurrentFireMode].Color;

        var i = 0;
        foreach (var layer in layers)
        {
            var key = layer.MapKeys?.FirstOrDefault();
            if (key == null)
            {
                key = i == 0 ? $"{args.Slot}-color" : $"{args.Slot}-color-{i}";
                i++;
            }

            layer.Color = color;
            args.Layers.Add((key, layer));
        }
    }
}
