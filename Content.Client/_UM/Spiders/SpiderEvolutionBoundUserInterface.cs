using Content.Client.UserInterface.Controls;
using Content.Shared._UM.Spiders.Evolution;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._UM.Spiders;


[UsedImplicitly]
public sealed partial class SpiderEvolutionBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private SimpleRadialMenu? _menu;
    [Dependency] private IPrototypeManager _prototypeManager = default!;

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<SimpleRadialMenu>();
        Update();
        _menu.OpenOverMouseScreenPosition();
    }

    public override void Update()
    {
        if (_menu == null)
            return;

        if (!EntMan.TryGetComponent<SpiderEvolutionComponent>(Owner, out var evolutionComponent))
            return;

        var models = ConvertToButtons(evolutionComponent.EvolutionTypes);

        _menu.SetButtons(models);
    }

    private IEnumerable<RadialMenuOptionBase> ConvertToButtons(List<EntProtoId> prototypes)
    {
        var buttons = new List<RadialMenuOptionBase>();

        foreach (var protoId in prototypes)
        {
            if (!_prototypeManager.TryIndex(protoId, out var prototype))
                continue;

            var entityName = prototype.Name;
            var entityDescription = prototype.Description;

            var option = new RadialMenuActionOption<EntProtoId>(SendIdentitySelect, protoId)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(prototype),
                ToolTip = entityName + " : " + entityDescription,
            };
            buttons.Add(option);
        }

        return buttons;
    }

    private void SendIdentitySelect(EntProtoId protoId)
    {
        SendPredictedMessage(new SpiderEvolutionSelectMessage(protoId));
    }

}
