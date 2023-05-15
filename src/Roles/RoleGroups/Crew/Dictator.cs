using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Vanilla.Meetings;
using Lotus.Chat;
using Lotus.Extensions;
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
    private int totalDictates;
    private int currentDictates;

    [UIComponent(UI.Counter, ViewMode.Replace, GameState.InMeeting)]
    private string DictateCounter() => RoleUtils.Counter(currentDictates, totalDictates, RoleColor);

    protected override void PostSetup() => currentDictates = totalDictates;

    [RoleAction(RoleActionType.MyVote)]
    private void DictatorVote(Optional<PlayerControl> target, MeetingDelegate meetingDelegate)
    {
        if (!target.Exists()) return;
        meetingDelegate.EndVoting(target.Get().Data);
        ChatHandler.Of(TranslationUtil.Colorize(DictateMessage.Formatted(target.Get().name, RoleName), RoleColor))
            .Title(t => t.Color(RoleColor).Text(RoleName).Build())
            .LeftAlign()
            .Send();
        
        Game.MatchData.GameHistory.AddEvent(new DictatorVoteEvent(MyPlayer, target.Get()));
        if (--currentDictates > 0) return;

        ProtectedRpc.CheckMurder(MyPlayer, MyPlayer);
        Game.MatchData.GameHistory.AddEvent(new SuicideEvent(MyPlayer));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Number of Dictates", NumberOfDictates)
                .AddIntRange(1, 15)
                .BindInt(i => totalDictates = i)
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
        public static string DictateMessage = "{0} was voted out by the {1}::1";

        [Localized("Options")]
        public static class DictatorOptionTranslations
        {
            [Localized(nameof(NumberOfDictates))]
            public static string NumberOfDictates = "Number of Dictates";
        }
    }
}