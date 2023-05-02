using System;
using System.Collections.Generic;
using TOHTOR.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;

namespace TOHTOR.Options.General;

[Localized("Options")]
public class GameplayOptions
{
    private static Color _optionColor = new(0.81f, 1f, 0.75f);
    private static List<GameOption> additionalOptions = new();

    public bool OptimizeRoleAssignment;

    public bool FixFirstKillCooldown;
    public bool DisableTasks;
    public DisabledTask DisabledTaskFlag;
    public bool DisableTaskWin;
    public bool GhostsSeeRoles;
    public bool GhostsSeeInfo;
    public bool GhostsIgnoreTasks;
    public bool ForceNoVenting;
    public int SyncMeetingCount;

    public bool SyncMeetings => SyncMeetingCount != -1;

    public List<GameOption> AllOptions = new();

    public GameplayOptions()
    {
        AllOptions.Add(new GameOptionTitleBuilder()
            .Title(GameplayOptionTranslations.GameplayOptionTitle)
            .Color(_optionColor)
            .Tab(DefaultTabs.GeneralTab)
            .Build());

        AllOptions.Add(Builder("Optimize Role Counts for Playability")
            .Name(GameplayOptionTranslations.OptimizeRoleAmounts)
            .AddOnOffValues()
            .BindBool(b => OptimizeRoleAssignment = b)
            .IsHeader(true)
            .BuildAndRegister());

        AllOptions.Add(Builder("Fix First Kill Cooldown")
            .Name(GameplayOptionTranslations.FixFirstKillCooldownText)
            .AddOnOffValues()
            .BindBool(b => FixFirstKillCooldown = b)
            .BuildAndRegister());

        AllOptions.Add(Builder("Disable Tasks")
            .Name(GameplayOptionTranslations.DisableTaskText)
            .AddOnOffValues(false)
            .ShowSubOptionPredicate(b => (bool)b)
            .SubOption(sub => sub
                .Key("Disable Card Swipe")
                .Name(GameplayOptionTranslations.DisableCardSwipe)
                .BindBool(FlagSetter(DisabledTask.CardSwipe))
                .AddOnOffValues()
                .Build())
            .SubOption(sub => sub
                .Key("Disable Med Scan")
                .Name(GameplayOptionTranslations.DisableMedScan)
                .BindBool(FlagSetter(DisabledTask.MedScan))
                .AddOnOffValues()
                .Build())
            .SubOption(sub => sub
                .Key("Disable Unlock Safe")
                .Name(GameplayOptionTranslations.DisableUnlockSafe)
                .BindBool(FlagSetter(DisabledTask.UnlockSafe))
                .AddOnOffValues()
                .Build())
            .SubOption(sub => sub
                .Key("Disable Upload Data")
                .Name(GameplayOptionTranslations.DisableUploadData)
                .BindBool(FlagSetter(DisabledTask.UploadData))
                .AddOnOffValues()
                .Build())
            .SubOption(sub => sub
                .Key("Disable Start Reactor")
                .Name(GameplayOptionTranslations.DisableStartReactor)
                .BindBool(FlagSetter(DisabledTask.StartReactor))
                .AddOnOffValues()
                .Build())
            .SubOption(sub => sub
                .Key("Disable Reset Breaker")
                .Name(GameplayOptionTranslations.DisableResetBreaker)
                .BindBool(FlagSetter(DisabledTask.ResetBreaker))
                .AddOnOffValues()
                .Build())
            .BindBool(b => DisableTasks = b)
            .BuildAndRegister());

        AllOptions.Add(Builder("Disable Task Win")
            .Name(GameplayOptionTranslations.DisableTaskWinText)
            .AddOnOffValues(false)
            .BindBool(b => DisableTaskWin = b)
            .BuildAndRegister());

        AllOptions.Add(Builder("Ghosts See Roles")
            .Name(GameplayOptionTranslations.GhostSeeRoles)
            .AddOnOffValues()
            .BindBool(b => GhostsSeeRoles = b)
            .BuildAndRegister());

        AllOptions.Add(Builder("Ghosts See Indicators")
            .Name(GameplayOptionTranslations.GhostSeeInfo)
            .AddOnOffValues()
            .BindBool(b => GhostsSeeInfo = b)
            .BuildAndRegister());

        AllOptions.Add(Builder("Ghosts Ignore Tasks")
            .Name(GameplayOptionTranslations.GhostIgnoreTask)
            .AddOnOffValues(false)
            .BindBool(b => GhostsIgnoreTasks = b)
            .BuildAndRegister());

        AllOptions.Add(Builder("Sync Meetings")
            .Name(GameplayOptionTranslations.SyncMeetingText)
            .Value(v => v.Text(GameplayOptionTranslations.SyncMeetingNever).Value(-1).Color(Color.red).Build())
            .AddIntRange(1, 15, 1)
            .BindInt(i => SyncMeetingCount = i)
            .BuildAndRegister());

        AllOptions.Add(Builder("Force No Venting")
            .Name(GameplayOptionTranslations.ForceNoVentingText)
            .AddOnOffValues()
            .BindBool(b => ForceNoVenting = b)
            .BuildAndRegister());

        additionalOptions.ForEach(o =>
        {
            o.Register();
            AllOptions.Add(o);
        });
    }

    /// <summary>
    /// Adds additional options to be registered when this group of options is loaded. This is mostly used for ordering
    /// in the main menu, as options passed in here will be rendered along with this group.
    /// </summary>
    /// <param name="option">Option to render</param>
    public static void AddAdditionalOption(GameOption option)
    {
        additionalOptions.Add(option);
    }

    private Action<bool> FlagSetter(DisabledTask disabledTask)
    {
        return b =>
        {
            if (b) DisabledTaskFlag |= disabledTask;
            else DisabledTaskFlag &= ~disabledTask;
        };
    }

    private GameOptionBuilder Builder(string key) => new GameOptionBuilder().Key(key).Tab(DefaultTabs.GeneralTab).Color(_optionColor);

    [Localized("Gameplay")]
    private static class GameplayOptionTranslations
    {

        [Localized("SectionTitle")]
        public static string GameplayOptionTitle = "Gameplay Options";

        [Localized(nameof(OptimizeRoleAmounts))]
        public static string OptimizeRoleAmounts = "Optimize Role Counts for Playability";

        [Localized("FixFirstKillCooldown")]
        public static string FixFirstKillCooldownText = "Fix First Kill Cooldown";

        [Localized("DisableTasks")]
        public static string DisableTaskText = "Disable Tasks";

        [Localized(nameof(DisableCardSwipe))]
        public static string DisableCardSwipe = "Disable Card Swipe";

        [Localized(nameof(DisableMedScan))]
        public static string DisableMedScan = "Disable Med Scan";

        [Localized(nameof(DisableUnlockSafe))]
        public static string DisableUnlockSafe = "Disable Unlock Safe";

        [Localized(nameof(DisableUploadData))]
        public static string DisableUploadData = "Disable Upload Data";

        [Localized(nameof(DisableStartReactor))]
        public static string DisableStartReactor = "Disable Start Reactor";

        [Localized(nameof(DisableResetBreaker))]
        public static string DisableResetBreaker = "Disable Reset Breaker";

        [Localized("DisableTaskWin")]
        public static string DisableTaskWinText = "Disable Task Win";

        [Localized(nameof(GhostSeeRoles))]
        public static string GhostSeeRoles = "Ghosts See Roles";

        [Localized(nameof(GhostSeeInfo))]
        public static string GhostSeeInfo = "Ghosts See Indicators";

        [Localized(nameof(GhostIgnoreTask))]
        public static string GhostIgnoreTask = "Ghosts Ignore Tasks";

        [Localized("SyncMeetingText")]
        public static string SyncMeetingText = "Sync Meetings";

        [Localized("SyncMeetingNever")]
        public static string SyncMeetingNever = "Never";

        [Localized("ForceNoVenting")]
        public static string ForceNoVentingText = "Force No Venting";
    }
}

[Flags]
public enum DisabledTask
{
    CardSwipe = 1,
    MedScan = 2,
    UnlockSafe = 4,
    UploadData = 8,
    StartReactor = 16,
    ResetBreaker = 32
}