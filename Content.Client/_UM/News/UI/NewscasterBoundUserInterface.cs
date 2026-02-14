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
        _window.OnOpen += WindowOnOnOpen;
    }

    private void WindowOnOnOpen()
    {
        if (_window != null)
            _window.OpenWindow();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (_window == null)
            return;

        switch (state)
        {
            case NewscasterBoundUserInterfaceState cast:
                _window.UpdateState(cast.Articles);
                break;
        }
    }
}

