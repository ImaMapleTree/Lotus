using System;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Processes;
using Lotus.API.Reactive;
using Lotus.Victory;
using Lotus.Patches.Actions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.RPC;
using UnityEngine;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.API.Vanilla.Meetings;

internal class MeetingPrep
{
    internal static DateTime MeetingCalledTime = DateTime.Now;
    internal static GameData.PlayerInfo? Reported;

    private static MeetingDelegate _meetingDelegate = null!;
    public static bool Prepped;

    private const string MeetingPrepHookKey = nameof(MeetingPrepHookKey);

    static MeetingPrep()
    {
        Hooks.GameStateHooks.GameStartHook.Bind(MeetingPrepHookKey, _ => Prepped = false);
        Hooks.GameStateHooks.RoundStartHook.Bind(MeetingPrepHookKey, _ => Prepped = false);
    }

    /// <summary>
    /// This API is a little bit strange, but basically if you provide the report the meeting will actually be called. Otherwise this guarantees meeting prep has been done and returns the most recent meeting delegate.
    /// </summary>
    /// <param name="reporter">Optional player, if provided, uses rpc to call meeting</param>
    /// <param name="deadBody">Optional reported body</param>
    /// <returns>the current meeting delegate</returns>
    public static MeetingDelegate? PrepMeeting(PlayerControl? reporter = null, GameData.PlayerInfo? deadBody = null)
    {
        if (!Prepped) _meetingDelegate = new MeetingDelegate();
        if (Prepped || !AmongUsClient.Instance.AmHost) return _meetingDelegate;
        ActionHandle handle = ActionHandle.NoInit();
        if (reporter != null) Game.TriggerForAll(RoleActionType.MeetingCalled, ref handle, reporter, Optional<GameData.PlayerInfo>.Of(deadBody));
        if (handle.IsCanceled) return null;

        Game.State = GameState.InMeeting;

        NameUpdateProcess.Paused = true;

        if (reporter != null)
            Async.Schedule(() => QuickStartMeeting(reporter), 0.1f);

        Game.RenderAllForAll(GameState.InMeeting, true);
        Async.Schedule(FixChatNames, NetUtils.DeriveDelay(4f));

        VentLogger.Trace("Finished Prepping", "MeetingPrep");
        Prepped = true;

        CheckEndGamePatch.Deferred = true;
        Game.SyncAll();
        return _meetingDelegate;
    }

    private static void QuickStartMeeting(PlayerControl reporter)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        MeetingCalledTime = DateTime.Now;
        MeetingRoomManager.Instance.AssignSelf(reporter, Reported);
        DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(reporter);
        reporter.RpcStartMeeting(Reported);
    }

    private static void FixChatNames() => Game.GetAllPlayers().ForEach(p => p.RpcSetName(Color.white.Colorize(p.name)));
}