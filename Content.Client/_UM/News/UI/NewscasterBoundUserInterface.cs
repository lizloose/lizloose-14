using Content.Shared._UM.News.Components;
using Robust.Client.UserInterface;

namespace Content.Client._UM.News.UI;

public sealed class NewscasterBoundUserInterface : BoundUserInterface
{
    private NewscasterWindow? _window;

    public NewscasterBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {

    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<NewscasterWindow>();

        if (!EntMan.TryGetComponent(Owner, out NewscasterComponent? component))
            return;

        _window.Update(component.Articles);
    }

    public override void Update()
    {
        if (_window == null)
            return;

        if (!EntMan.TryGetComponent(Owner, out NewscasterComponent? component))
            return;

        _window.Update(component.Articles);
    }
}

