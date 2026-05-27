using Content.Shared._UM.Drip.Components;
using JetBrains.Annotations;
using Robust.Client.Player;
using Robust.Client.UserInterface;

namespace Content.Client._UM.Drip.UI
{
    [UsedImplicitly]
    public sealed partial class DripDrobeBoundUserInterface : BoundUserInterface
    {
        [Dependency] private DripTrackingManager _dripTrackingManager = default!;
        [Dependency] private IPlayerManager _playerManager = default!;


        private DripDrobeWindow? _window;

        public DripDrobeBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<DripDrobeWindow>();
            _window.OnOpen += OnOpened;
            Refresh();
        }

        public void Refresh()
        {
            if (_window is not { } menu)
                return;

            var session = _playerManager.LocalSession;
            if (session == null)
                return;

            var drip = _dripTrackingManager.GetAvailableEntities(session);

            menu.Populate(drip);

            _window.OnItemSelected += (_, item) =>
            {
                SendMessage(new DripDrobeDispenseItemMessage(item));
            };
        }

        private void OnOpened()
        {

        }
    }
}
