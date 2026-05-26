using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Shared._UM.Drip;
using Robust.Shared.Asynchronous;
using Robust.Shared.Collections;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._UM.Drip;

public sealed partial class DripTrackingManager : ISharedDripTrackingManager, IPostInjectInit
{
    [Dependency] private UserDbDataManager _userDb = default!;
    [Dependency] private IServerDbManager _db = default!;
    [Dependency] private IServerNetManager _net = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private ITaskManager _task = default!;

    private ISawmill _sawmill = default!;

    // DB auto-saving logic.
    private TimeSpan _saveInterval = TimeSpan.FromSeconds(5);
    private TimeSpan _lastSave;

    private ValueList<ICommonSession> _playersDirty;

    private readonly List<Task> _pendingSaveTasks = new();
    private readonly Dictionary<ICommonSession, DripData> _dripData = new();

    public void Initialize()
    {
        _sawmill = Logger.GetSawmill("drip_tracking");

        _net.RegisterNetMessage<MsgDripData>();
    }

    public void Shutdown()
    {
        Save();

        _task.BlockWaitOnTask(Task.WhenAll(_pendingSaveTasks));
    }

    public void Update()
    {
        UpdateDirtyPlayers();

        if (_timing.RealTime < _lastSave + _saveInterval)
            return;

        Save();
    }

    /// <summary>
    /// Save all modified trackers for all players to the database.
    /// </summary>
    public async void Save()
    {
        //FlushAllTrackers();

        _lastSave = _timing.RealTime;

        TrackPending(DoSaveAsync());
    }

    /// <summary>
    /// Track a database save task to make sure we block server shutdown on it.
    /// </summary>
    private async void TrackPending(Task task)
    {
        _pendingSaveTasks.Add(task);

        try
        {
            await task;
        }
        finally
        {
            _pendingSaveTasks.Remove(task);
        }
    }

    private async Task DoSaveAsync()
    {
        var log = new List<DripTrackingUpdate>();

        foreach (var (player, data) in _dripData)
        {
            foreach (var tracker in data.DbTrackersDirty)
            {
                log.Add(new DripTrackingUpdate(player.UserId, tracker, data.AvailableEntities[tracker]));
            }

            data.DbTrackersDirty.Clear();
        }

        if (log.Count == 0)
            return;

        _sawmill.Debug($"Saved {log.Count} trackers");
    }

    private async Task DoSaveSessionAsync(ICommonSession session)
    {
        var log = new List<DripTrackingUpdate>();

        var data = _dripData[session];

        foreach (var tracker in data.DbTrackersDirty)
        {
            log.Add(new DripTrackingUpdate(session.UserId, tracker, data.AvailableEntities[tracker]));
        }

        data.DbTrackersDirty.Clear();

        await _db.UpdateDrip(log);
        _sawmill.Debug($"Saved drip trackers for {session.Name}");
    }

    public async Task LoadData(ICommonSession session, CancellationToken cancel)
    {
        var data = new DripData();
        _dripData.Add(session, data);

        var trackedDrips = await _db.GetDrip(session.UserId, cancel);
        cancel.ThrowIfCancellationRequested();

        foreach (var drip in trackedDrips)
        {
            data.AvailableEntities.Add(drip.DripName, drip.RoundsLeft);
        }

        data.Initialized = true;

       QueueSendUpdate(session);
    }

    /// <summary>
    /// Queue for tracked information to be sent to a client, for showing in UIs etc.
    /// </summary>
    public void QueueSendUpdate(ICommonSession player)
    {
        if (DirtyPlayer(player) is { } data)
            data.NeedSendTimers = true;
    }

    private DripData? DirtyPlayer(ICommonSession player)
    {
        if (!_dripData.TryGetValue(player, out var data) || !data.Initialized)
            return null;

        if (!data.IsDirty)
        {
            data.IsDirty = true;
            _playersDirty.Add(player);
        }

        return data;
    }

    public void ClientDisconnected(ICommonSession session)
    {
        SaveSession(session);

        _dripData.Remove(session);
    }

    private void UpdateDirtyPlayers()
    {
        if (_playersDirty.Count == 0)
            return;

        foreach (var player in _playersDirty)
        {
            if (!_dripData.TryGetValue(player, out var data))
                continue;

            DebugTools.Assert(data.IsDirty);

            if (data.NeedSendTimers)
            {
                SendDripData(player);
                data.NeedSendTimers = false;
            }

            SendDripData(player);
            data.IsDirty = false;
        }

        _playersDirty.Clear();
    }

    private void SendDripData(ICommonSession pSession)
    {
        var drips = GetTrackedDrip(pSession);

        var msg = new MsgDripData
        {
            AvailableDrip = drips
        };

        _net.ServerSendMessage(msg, pSession.Channel);
    }

    public IReadOnlyDictionary<string, int> GetAvailableEntities(ICommonSession session)
    {
        return GetTrackedDrip(session);
    }

    public Dictionary<string, int> GetTrackedDrip(ICommonSession id)
    {
        if (!_dripData.TryGetValue(id, out var data) || !data.Initialized)
            throw new InvalidOperationException("Drip info is not yet loaded for this player!");

        return data.AvailableEntities;
    }

    public bool TryGetTrackedDrip(ICommonSession id, [NotNullWhen(true)] out Dictionary<string, int>? drip)
    {
        drip = null;

        if (!_dripData.TryGetValue(id, out var data) || !data.Initialized)
        {
            return false;
        }

        drip = data.AvailableEntities;
        return true;
    }

    public void AddRoundsToTracker(ICommonSession id, string entId, int redemptions)
    {
        if (!_dripData.TryGetValue(id, out var data) || !data.Initialized)
            return;

        ref var rounds = ref CollectionsMarshal.GetValueRefOrAddDefault(data.AvailableEntities, entId, out _);
        rounds += redemptions;

        data.DbTrackersDirty.Add(entId);
    }

    public void SetDripRounds(ICommonSession id, string entId, int redemptions)
    {
        if (!_dripData.TryGetValue(id, out var data) || !data.Initialized)
            return;

        ref var rounds = ref CollectionsMarshal.GetValueRefOrAddDefault(data.AvailableEntities, entId, out _);
        rounds = redemptions;

        data.DbTrackersDirty.Add(entId);
        QueueSendUpdate(id);
    }

    /// <summary>
    /// Save all modified time trackers for a player to the database.
    /// </summary>
    public async void SaveSession(ICommonSession session)
    {
        TrackPending(DoSaveSessionAsync(session));
    }


    /// <summary>
    /// Drip data for a particular player
    /// </summary>
    private sealed class DripData
    {
        // Queued update flags
        public bool IsDirty;
        public bool NeedSendTimers;

        // Stored tracked time info.

        /// <summary>
        /// Have we finished retrieving our data from the DB?
        /// </summary>
        public bool Initialized;

        /// <summary>
        /// Dictionary of unlocked items and the redemptions left
        /// </summary>
        public readonly Dictionary<string, int> AvailableEntities = new();

        /// <summary>
        /// Set of trackers which are different from their DB values and need to be saved to DB.
        /// </summary>
        public readonly HashSet<string> DbTrackersDirty = new();
    }

    void IPostInjectInit.PostInject()
    {
        _userDb.AddOnLoadPlayer(LoadData);
        _userDb.AddOnPlayerDisconnect(ClientDisconnected);
    }
}
