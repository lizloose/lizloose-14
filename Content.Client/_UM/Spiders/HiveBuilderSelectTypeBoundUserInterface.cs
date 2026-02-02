using Content.Client.UserInterface.Controls;
using Content.Shared._UM.Spiders.Builder;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._UM.Spiders;


[UsedImplicitly]
public sealed class HiveBuilderSelectTypeBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private SimpleRadialMenu? _menu;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

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

        if (!EntMan.TryGetComponent<HiveBuilderComponent>(Owner, out var hiveBuilderComponent))
            return;

        var models = ConvertToButtons(hiveBuilderComponent.BuildTypes);

        _menu.SetButtons(models);
    }

    private IEnumerable<RadialMenuOptionBase> ConvertToButtons(List<ProtoId<HiveBuildPrototype>> prototypes)
    {
        var buttons = new List<RadialMenuOptionBase>();

        foreach (var protoId in prototypes)
        {
            var prototype = _prototypeManager.Index(protoId);

            var option = new RadialMenuActionOption<HiveBuildPrototype>(SendIdentitySelect, prototype)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(prototype.Prototype),
                ToolTip = prototype.SetName
            };
            buttons.Add(option);
        }

        return buttons;
    }

    private void SendIdentitySelect(HiveBuildPrototype protoId)
    {
        SendPredictedMessage(new HiveBuilderTypeSelectMessage(protoId));
    }

}
