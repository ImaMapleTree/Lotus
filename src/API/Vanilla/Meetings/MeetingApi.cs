using System;
using TOHTOR.API.Reactive;
using VentLib.Utilities.Optionals;

// ReSharper disable InconsistentNaming

namespace TOHTOR.API.Vanilla.Meetings;

public class MeetingApi
{
    public static void StartMeeting(Func<ReporterSetter, MeetingCreator> creationFunction) => creationFunction(new MeetingCreator()).BeginMeeting();

    public class MeetingCreator : ReporterSetter, SubjectSetter, DelegateEnforcer
    {
        private const string MeetingCreatorHookKey = nameof(MeetingCreator);
        private PlayerControl caller = null!;
        private Optional<PlayerControl> subject = Optional<PlayerControl>.Null();
        private Action<MeetingDelegate>? consumer;

        public SubjectSetter Caller(PlayerControl reporter)
        {
            caller = reporter;
            return this;
        }

        public MeetingCreator QuickCall(PlayerControl reporter)
        {
            caller = reporter;
            return this;
        }

        public DelegateEnforcer Subject(PlayerControl reporter)
        {
            subject = Optional<PlayerControl>.Of(reporter);
            return this;
        }

        public DelegateEnforcer NoSubject()
        {
            subject = Optional<PlayerControl>.Null();
            return this;
        }

        public MeetingCreator Callback(Action<MeetingDelegate> consumer)
        {
            this.consumer = consumer;
            return this;
        }

        public MeetingCreator Ignore() => this;

        internal void BeginMeeting()
        {
            Hooks.MeetingHooks.MeetingCalledHook.Bind(MeetingCreatorHookKey, hookEvent => consumer?.Invoke(hookEvent.Delegate), true);
            caller.CmdReportDeadBody(subject.Map(p => p.Data).OrElse(null!));
        }
    }
}

public interface ReporterSetter
{
    public SubjectSetter Caller(PlayerControl reporter);
    public MeetingApi.MeetingCreator QuickCall(PlayerControl reporter);
}

public interface SubjectSetter
{
    public DelegateEnforcer Subject(PlayerControl player);
    public DelegateEnforcer NoSubject();
}

public interface DelegateEnforcer
{
    public MeetingApi.MeetingCreator Callback(Action<MeetingDelegate> consumer);
    public MeetingApi.MeetingCreator Ignore();
}