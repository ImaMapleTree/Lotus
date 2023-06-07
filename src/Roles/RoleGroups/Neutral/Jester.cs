using System.Collections.Generic;
using AmongUs.GameOptions;
using Lotus.API;
using Lotus.API.Stats;
using Lotus.Factions;
using Lotus.Options;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Victory.Conditions;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Roles.Internals;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.RoleGroups.Neutral;

public class Jester : CustomRole
{
    private bool canUseVents;
    private bool impostorVision;
    private int meetingThreshold;
    public bool cantCallMeetings;

    private int meetingsCalled;

    [UIComponent(UI.Counter)]
    public string MeetingCounter() => meetingThreshold > 0 ? RoleUtils.Counter(meetingsCalled, meetingThreshold, RoleColor) : "";

    [RoleAction(RoleActionType.SelfExiled)]
    public void JesterWin()
    {
        if (meetingsCalled < meetingThreshold) return;
        VentLogger.Fatal("Forcing Win by Jester");
        ManualWin jesterWin = new(MyPlayer, WinReason.SoloWinner, 999);
        jesterWin.Activate();
    }

    [RoleAction(RoleActionType.MeetingCalled)]
    public void CheckCallMeeting(PlayerControl caller, ActionHandle handle, Optional<GameData.PlayerInfo> deadBody)
    {
        if (caller.PlayerId != MyPlayer.PlayerId) return;
        // Cancel if the jester can't call emergency meetings
        if (!deadBody.Exists() && cantCallMeetings) handle.Cancel();
    }

    [RoleAction(RoleActionType.MeetingEnd)]
    public void MeetingIncrementer()
    {
        meetingsCalled++;
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .Tab(DefaultTabs.NeutralTab)
            .SubOption(opt =>
                opt.KeyName("Has Impostor Vision", Translations.Options.ImpostorVision)
                    .Bind(v => impostorVision = (bool)v).AddOnOffValues().Build())
            .SubOption(opt => opt.KeyName("Can Use Vents", Translations.Options.CanUseVents)
                .Bind(v => canUseVents = (bool)v)
                .AddOnOffValues()
                .Build())
            .SubOption(sub => sub.KeyName("Can't Call Emergency Meetings", Translations.Options.CantCallEmergencyMeetings)
                .BindBool(b => cantCallMeetings = b)
                .AddOnOffValues(false)
                .Build())
            .SubOption(sub => sub.KeyName("Minimum Meetings Before Win", Translations.Options.MeetingsBeforeWinning)
                .AddIntRange(0, 30)
                .BindInt(i => meetingThreshold = i)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier)
    {
        return roleModifier
            .Faction(FactionInstances.Neutral)
            .VanillaRole(canUseVents ? RoleTypes.Engineer : RoleTypes.Crewmate)
            .SpecialType(SpecialType.Neutral)
            .RoleFlags(RoleFlag.CannotWinAlone)
            .RoleColor(new Color(0.93f, 0.38f, 0.65f))
            .OptionOverride(Override.CrewLightMod, () => AUSettings.ImpostorLightMod(), () => impostorVision)
            .OptionOverride(Override.EngVentDuration, 100f)
            .OptionOverride(Override.EngVentCooldown, 0.1f);
    }

    public override List<Statistic> Statistics() => new() {VanillaStatistics.TimesVented };

    [Localized(nameof(Jester))]
    private static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(ImpostorVision))]
            public static string ImpostorVision = "Has Impostor Vision";

            [Localized(nameof(CanUseVents))]
            public static string CanUseVents = "Can Use Vents";

            [Localized(nameof(CantCallEmergencyMeetings))]
            public static string CantCallEmergencyMeetings = "Can't Call Emergency Meetings";

            [Localized(nameof(MeetingsBeforeWinning))]
            public static string MeetingsBeforeWinning = "Minimum Meetings Before Ability to Win";
        }
    }
}