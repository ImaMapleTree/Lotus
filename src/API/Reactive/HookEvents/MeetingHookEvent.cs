using TOHTOR.API.Meetings;

namespace TOHTOR.API.Reactive.HookEvents;

public class MeetingHookEvent : IHookEvent
{
    public MeetingDelegate Delegate;

    public MeetingHookEvent(MeetingDelegate meetingDelegate)
    {
        Delegate = meetingDelegate;
    }
}