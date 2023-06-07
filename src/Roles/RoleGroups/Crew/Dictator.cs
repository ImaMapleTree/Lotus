using System.Collections.Generic;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Stats;
using Lotus.API.Vanilla.Meetings;
using Lotus.Chat;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Managers.History.Events;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using static Lotus.Roles.RoleGroups.Crew.Dictator.DictatorTranslations;
using static Lotus.Roles.RoleGroups.Crew.Dictator.DictatorTranslations.DictatorOptionTranslations;

namespace Lotus.Roles.RoleGroups.Crew;

public class Dictator: Crewmate
{
    private static IAccumulativeStatistic<int> _playersEjected = Statistic<int>.CreateAccumulative("Roles.Dictator.PlayersEjected", () => PlayersEjectedStat);
    public override List<Statistic> Statistics() => new() { _playersEjected };

    private bool suicideIfVoteCrewmate;
    private int totalDictates;

    private int currentDictates;


    [UIComponent(UI.Counter, ViewMode.Replace, GameState.InMeeting)]
    private string DictateCounter() => RoleUtils.Counter(currentDictates, totalDictates, RoleColor);

    protected override void PostSetup() => currentDictates = totalDictates;

    [RoleAction(RoleActionType.MyVote)]
    private void DictatorVote(Optional<PlayerControl> target, MeetingDelegate meetingDelegate)
    {
        if (!target.Exists()) return;
        PlayerControl player = target.Get();
        meetingDelegate.EndVoting(player.Data);
        ChatHandler.Of(TranslationUtil.Colorize(DictateMessage.Formatted(player.name, RoleName), RoleColor))
            .Title(t => t.Color(RoleColor).Text(RoleName).Build())
            .LeftAlign()
            .Send();

        _playersEjected.Increment(MyPlayer.UniquePlayerId());
        Game.MatchData.GameHistory.AddEvent(new DictatorVoteEvent(MyPlayer, player));

        // I hate this negation but basically:
        // If remaining > 0
        //     AND
        // Not Suicide on Voting Crewmates
        //     OR
        // Target relationship is not allied
        // Then: Return
        if (--currentDictates > 0 && (!suicideIfVoteCrewmate || Relationship(player) is not Relation.FullAllies)) return;

        ProtectedRpc.CheckMurder(MyPlayer, MyPlayer);
        Game.MatchData.GameHistory.AddEvent(new SuicideEvent(MyPlayer));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Number of Dictates", NumberOfDictates)
                .AddIntRange(1, 15)
                .BindInt(i => totalDictates = i)
                .Build())
            .SubOption(sub => sub.KeyName("Suicide if Crewmate Executed", TranslationUtil.Colorize(SuicideIfVoteCrewmate, ModConstants.Palette.CrewmateColor))
                .AddOnOffValues(false)
                .BindBool(b => suicideIfVoteCrewmate = b)
                .Build());


    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.87f, 0.61f, 0f));

    private class DictatorVoteEvent : KillEvent, IRoleEvent
    {
        public DictatorVoteEvent(PlayerControl killer, PlayerControl victim) : base(killer, victim)
        {
        }

        public override string Message() => TranslationUtil.Colorize(DictateMessage.Formatted(Player().name, Target().name), Player().GetRoleColor(), Target().GetRoleColor());
    }

    [Localized(nameof(Dictator))]
    internal static class DictatorTranslations
    {
        [Localized(nameof(LynchEventMessage))]
        public static string LynchEventMessage = "{0}::0 lynched {1}::1.";

        [Localized(nameof(DictateMessage))]
        public static string DictateMessage = "{0} was voted out by the {1}::0";

        [Localized(nameof(PlayersEjectedStat))]
        public static string PlayersEjectedStat = "Players Ejected";

        [Localized(ModConstants.Options)]
        public static class DictatorOptionTranslations
        {
            [Localized(nameof(NumberOfDictates))]
            public static string NumberOfDictates = "Number of Dictates";

            [Localized(nameof(SuicideIfVoteCrewmate))]
            public static string SuicideIfVoteCrewmate = "Suicide if Crewmate::0 Executed";
        }
    }
}