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
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.API.Vanilla.Meetings;

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
    public static MeetingDelegate? PrepMeeting(PlayerControl? reporter = null)
    {
        if (!_prepped) _meetingDelegate = new MeetingDelegate();
        if (_prepped || !AmongUsClient.Instance.AmHost) return _meetingDelegate;
        ActionHandle handle = ActionHandle.NoInit();
        if (reporter != null) Game.TriggerForAll(RoleActionType.MeetingCalled, ref handle, reporter);
        if (handle.IsCanceled) return null;
        
        Game.State = GameState.InMeeting;

        NameUpdateProcess.Paused = true;

        if (reporter != null)
        {
            Game.GetAllPlayers().Where(p => p.IsShapeshifted()).ForEach(p => p.CRpcRevertShapeshift(false));
            Async.Schedule(() => QuickStartMeeting(reporter), NetUtils.DeriveDelay(0.2f));
        }
        
        Game.RenderAllForAll(GameState.InMeeting, true);
        Async.Schedule(FixChatNames, NetUtils.DeriveDelay(4f));

        VentLogger.Trace("Finished Prepping", "MeetingPrep");
        _prepped = true;
        
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

    private static void FixChatNames() => Game.GetAllPlayers().ForEach(p => p.RpcSetName(p.name));
}