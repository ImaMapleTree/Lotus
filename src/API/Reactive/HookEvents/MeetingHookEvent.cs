using Lotus.API.Vanilla.Meetings;

namespace Lotus.API.Reactive.HookEvents;

public class MeetingHookEvent : IHookEvent
{
    public PlayerControl Caller;
    public GameData.PlayerInfo? Reported;
    public MeetingDelegate Delegate;

    public MeetingHookEvent(PlayerControl caller, GameData.PlayerInfo? reporter, MeetingDelegate meetingDelegate)
    {
        Caller = caller;
        Reported = reporter;
        Delegate = meetingDelegate;
    }
}