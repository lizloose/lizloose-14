using Content.Shared._UM.Drip;
using Robust.Client;
using Robust.Client.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Client._UM.Drip;

public sealed partial class DripTrackingManager : ISharedDripTrackingManager
{
    [Dependency] private IBaseClient _client = default!;
    [Dependency] private IClientNetManager _net = default!;
    [Dependency] private IPlayerManager _playerManager = default!;

    private readonly Dictionary<string, int> _drip = new();

    public event Action? Updated;

    public void Initialize()
    {
        _net.RegisterNetMessage<MsgDripData>(RxPlayTime);
        _client.RunLevelChanged += ClientOnRunLevelChanged;
    }

    private void ClientOnRunLevelChanged(object? sender, RunLevelChangedEventArgs e)
    {
        if (e.NewLevel == ClientRunLevel.Initialize)
        {
            // Reset on disconnect, just in case.
            _drip.Clear();
        }
    }

    private void RxPlayTime(MsgDripData message)
    {
        _drip.Clear();

        foreach (var (tracker, time) in message.AvailableDrip)
        {
            _drip[tracker] = time;
        }

        Updated?.Invoke();
    }

    public IReadOnlyDictionary<string, int> GetAvailableEntities(ICommonSession session)
    {
        if (session != _playerManager.LocalSession)
        {
            return new Dictionary<string, int>();
        }

        return _drip;
    }

}
