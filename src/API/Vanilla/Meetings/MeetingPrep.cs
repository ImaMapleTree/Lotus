using System;
using TOHTOR.API.Odyssey;
using TOHTOR.API.Processes;
using TOHTOR.API.Reactive;
using TOHTOR.API.Reactive.HookEvents;
using TOHTOR.Extensions;
using TOHTOR.Victory;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace TOHTOR.API.Vanilla.Meetings;

internal class MeetingPrep
{
    internal static DateTime MeetingCalledTime = DateTime.Now;
    internal static GameData.PlayerInfo? Reported;

    private static MeetingDelegate _meetingDelegate = null!;
    private static bool _prepped;

    private const string MeetingPrepHookKey = nameof(MeetingPrepHookKey);

    static MeetingPrep()
    {
        Hooks.GameStateHooks.GameStartHook.Bind(MeetingPrepHookKey, _ => _prepped = false);
        Hooks.GameStateHooks.RoundStartHook.Bind(MeetingPrepHookKey, _ => _prepped = false);
    }

    /// <summary>
    /// This API is a little bit strange, but basically if you provide the report the meeting will actually be called. Otherwise this guarantees meeting prep has been done and returns the most recent meeting delegate.
    /// </summary>
    /// <param name="reporter">Optional player, if provided, uses rpc to call meeting</param>
    /// <returns>the current meeting delegate</returns>
    public static MeetingDelegate PrepMeeting(PlayerControl? reporter = null)
    {
        if (!_prepped) _meetingDelegate = new MeetingDelegate();
        if (_prepped || !AmongUsClient.Instance.AmHost) return _meetingDelegate;
        Game.State = GameState.InMeeting;

        NameUpdateProcess.Paused = true;
        Game.RenderAllForAll(GameState.InMeeting, true);
        Async.Schedule(FixChatNames, NetUtils.DeriveDelay(4f));

        _prepped = true;
        VentLogger.Trace("Finished Prepping", "MeetingPrep");
        MeetingCalledTime = DateTime.Now;

        if (reporter != null) Async.Schedule(() => QuickStartMeeting(reporter), NetUtils.DeriveDelay(0.2f));

        CheckEndGamePatch.Deferred = true;
        Hooks.GameStateHooks.RoundEndHook.Propagate(new GameStateHookEvent(Game.MatchData));
        Game.SyncAll();
        return _meetingDelegate;
    }

    private static void QuickStartMeeting(PlayerControl reporter)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        MeetingRoomManager.Instance.AssignSelf(reporter, Reported);
        DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(reporter);
        reporter.RpcStartMeeting(Reported);
    }

    private static void FixChatNames() => Game.GetAllPlayers().ForEach(p => p.RpcSetName(p.name));
}