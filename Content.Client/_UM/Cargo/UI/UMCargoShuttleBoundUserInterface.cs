using Content.Shared._UM.Cargo.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._UM.Cargo.UI;


[UsedImplicitly]

public sealed class UMCargoShuttleBoundUserInterface : BoundUserInterface
{
    private UMCargoShuttleWindow? _window;

    public UMCargoShuttleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<UMCargoShuttleWindow>();
        _window.OnSendButton += ButtonPressed;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        var castState = (UMCargoShuttleBoundUserInterfaceState) state;
        _window?.UpdateState(castState);
    }

    public void ButtonPressed()
    {
        SendMessage(new UMSendCargoShuttleMessage());
    }

}
