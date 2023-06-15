using System.Collections.Generic;
using HarmonyLib;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Options;
using Lotus.Roles.Internals;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.Impostors;

public class TimeThief : Vanilla.Impostor
{
    private int kills;
    private int meetingTimeSubtractor;
    private int minimumVotingTime;
    private bool returnTimeAfterDeath;

    private List<IRemote>? discussionRemote;
    private List<IRemote>? votingRemote;

    [UIComponent(UI.Counter)]
    public string TimeStolenCounter() => RoleUtils.Counter(kills * meetingTimeSubtractor + "s", color: RoleColor);

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        var flag = base.TryKill(target);
        if (flag)
            kills++;
        return flag;
    }

    [RoleAction(RoleActionType.MeetingCalled, triggerAfterDeath: true)]
    private void TimeThiefSubtractMeetingTime()
    {
        discussionRemote?.ForEach(d => d.Delete());
        votingRemote?.ForEach(v => v.Delete());
        if (!MyPlayer.IsAlive() && returnTimeAfterDeath) return;
        int discussionTime = AUSettings.DiscussionTime();
        int votingTime = AUSettings.VotingTime();

        int totalStolenTime = meetingTimeSubtractor * kills;

        // Total Meeting Time - Stolen Time = Remaining Meeting Time
        int modifiedDiscussionTime = discussionTime - totalStolenTime;

        if (modifiedDiscussionTime < 0)
        {
            totalStolenTime = -modifiedDiscussionTime;
            modifiedDiscussionTime = 1;
        }

        int modifiedVotingTime = Mathf.Clamp(votingTime - totalStolenTime, minimumVotingTime, votingTime);

        VentLogger.Debug($"{MyPlayer.name} | Time Thief | Meeting Time: {modifiedDiscussionTime} | Voting Time: {modifiedVotingTime}", "TimeThiefStolen");

        discussionRemote = new List<IRemote>();
        votingRemote = new List<IRemote>();
        Players.GetPlayers().ForEach(p =>
        {
            discussionRemote.Add(Game.MatchData.Roles.AddOverride(p.PlayerId, new GameOptionOverride(Override.DiscussionTime, modifiedDiscussionTime)));
            votingRemote.Add(Game.MatchData.Roles.AddOverride(p.PlayerId, new GameOptionOverride(Override.VotingTime, modifiedVotingTime)));
        });

    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Meeting Time Stolen")
                .Bind(v => meetingTimeSubtractor = (int)v)
                .AddIntRange(5, 120, 5, 4, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .Name("Minimum Voting Time")
                .Bind(v => minimumVotingTime = (int)v)
                .AddIntRange(5, 120, 5, 1, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .Name("Return Stolen Time After Death")
                .Bind(v => returnTimeAfterDeath = (bool)v)
                .AddOnOffValues()
                .Build());
}