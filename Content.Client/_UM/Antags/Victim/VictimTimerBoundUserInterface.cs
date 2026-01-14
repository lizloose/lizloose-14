using System.Numerics;
using Content.Shared._UM.Antags.Victim.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._UM.Antags.Victim;


[UsedImplicitly]
public sealed class VictimTimerBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private VictimTimerGui? _window;

    public VictimTimerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateDisposableControl<VictimTimerGui>();
        _window.Open();
        _window.Visible = true;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (_window == null || state is not VictimTimerBoundUserInterfaceState cast)
            return;

        _window.Update(cast.DetonationTime);
    }
}
