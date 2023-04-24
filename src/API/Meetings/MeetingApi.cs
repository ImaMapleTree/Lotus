using System;
using System.Collections.Generic;
using System.Linq;
using TOHTOR.API.Reactive;
using TOHTOR.Managers;
using TOHTOR.Managers.History.Events;
using TOHTOR.Patches.Meetings;
using TOHTOR.RPC;
using TOHTOR.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

// ReSharper disable InconsistentNaming

namespace TOHTOR.API.Meetings;

public class MeetingApi
{
    public static void StartMeeting(Func<ReporterSetter, MeetingCreator> creationFunction) => creationFunction(new MeetingCreator()).BeginMeeting();

    public static void EndVoting(MeetingHud meetingHud, MeetingHud.VoterState[] voterStates, GameData.PlayerInfo? exiledPlayer, bool tie)
    {
        AntiBlackout.SaveCosmetics();
        GameData.PlayerInfo? fakeExiled = AntiBlackout.CreateFakePlayer(exiledPlayer);

        AntiBlackout.ExiledPlayer = exiledPlayer;

        if (fakeExiled == null)
        {
            meetingHud.RpcVotingComplete(voterStates, null, true);
            return;
        }

        meetingHud.ComplexVotingComplete(voterStates, fakeExiled, tie); //通常処理
        List<PlayerControl> voters = voterStates.Where(s => s.VotedForId == exiledPlayer!.PlayerId)
            .Filter(s => Utils.PlayerById(s.VoterId))
            .ToList();
        List<PlayerControl> abstainers = voterStates.Where(s => s.VotedForId != exiledPlayer!.PlayerId)
            .Filter(s => Utils.PlayerById(s.VoterId))
            .ToList();
        Game.GameHistory.AddEvent(new ExiledEvent(exiledPlayer!.Object, voters, abstainers));
    }

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