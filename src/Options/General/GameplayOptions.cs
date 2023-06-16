using System;
using System.Collections.Generic;
using Lotus.API;
using Lotus.Extensions;
using Lotus.Roles.Overrides;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;

namespace Lotus.Options.General;

[Localized(ModConstants.Options)]
public class GameplayOptions
{
    private static Color _optionColor = new(0.81f, 1f, 0.75f);
    private static List<GameOption> additionalOptions = new();

    public bool OptimizeRoleAssignment;

    public FirstKillCooldown FirstKillCooldownMode;
    private float setCooldown;

    public bool DisableTasks;
    public DisabledTask DisabledTaskFlag;
    public bool DisableTaskWin;
    public bool GhostsSeeInfo;

    public int LadderDeathChance = -1;
    public bool EnableLadderDeath => LadderDeathChance > 0;

    public ModifierTextMode ModifierTextMode;

    public float GetFirstKillCooldown(PlayerControl player)
    {
        return FirstKillCooldownMode switch
        {
            FirstKillCooldown.SetCooldown => setCooldown,
            FirstKillCooldown.GlobalCooldown => AUSettings.KillCooldown(),
            FirstKillCooldown.RoleCooldown => player.GetCustomRole().GetOverride(Override.KillCooldown)?.GetValue() as float? ?? AUSettings.KillCooldown(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

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

        AllOptions.Add(Builder("First Kill Cooldown")
            .Name(GameplayOptionTranslations.FirstKillCooldown)
            .Value(v => v.Text(GameplayOptionTranslations.GlobalCooldown).Color(ModConstants.Palette.GlobalColor).Value(0).Build())
            .Value(v => v.Text(GameplayOptionTranslations.SetCooldown).Color(Color.green).Value(1).Build())
            .Value(v => v.Text(GameplayOptionTranslations.RoleCooldown).Color(ModConstants.Palette.InfinityColor).Value(2).Build())
            .BindInt(b => FirstKillCooldownMode = (FirstKillCooldown)b)
            .ShowSubOptionPredicate(i => (int)i == 1)
            .SubOption(sub => sub.Key("Set Cooldown Value").Name(GameplayOptionTranslations.SetCooldownValue)
                .AddFloatRange(0, 120, 2.5f, 4, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(f => setCooldown = f)
                .Build())
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
            .SubOption(sub => sub
                .Key("Disable Fix Wiring")
                .Name(GameplayOptionTranslations.DisableFixWiring)
                .BindBool(FlagSetter(DisabledTask.FixWiring))
                .AddOnOffValues()
                .Build())
            .BindBool(b => DisableTasks = b)
            .BuildAndRegister());

        AllOptions.Add(Builder("Disable Task Win")
            .Name(GameplayOptionTranslations.DisableTaskWinText)
            .AddOnOffValues(false)
            .BindBool(b => DisableTaskWin = b)
            .BuildAndRegister());

        AllOptions.Add(Builder("Ghosts See Info")
            .Name(GameplayOptionTranslations.GhostSeeInfo)
            .AddOnOffValues()
            .BindBool(b => GhostsSeeInfo = b)
            .BuildAndRegister());

        AllOptions.Add(Builder("Ladder Death")
            .Name(GameplayOptionTranslations.LadderDeathText)
            .Value(v => v.Text(GeneralOptionTranslations.OffText).Value(-1).Color(Color.red).Build())
            .AddIntRange(10, 100, 5, suffix: "%")
            .BindInt(i => LadderDeathChance = i)
            .BuildAndRegister());

        AllOptions.Add(Builder("Modifier Text Mode")
            .Name(GameplayOptionTranslations.ModifierTextMode)
            .Value(v => v.Text(GeneralOptionTranslations.OffText).Value(1).Color(Color.red).Build())
            .Value(v => v.Text(GameplayOptionTranslations.FirstValue).Value(0).Color(ModConstants.Palette.InfinityColor).Build())
            .Value(v => v.Text(GeneralOptionTranslations.AllText).Value(2).Color(Color.green).Build())
            .BindInt(i => ModifierTextMode = (ModifierTextMode)i)
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

        [Localized(nameof(FirstKillCooldown))]
        public static string FirstKillCooldown = "First Kill Cooldown";

        [Localized(nameof(SetCooldown))]
        public static string SetCooldown = "Set CD";

        [Localized(nameof(SetCooldownValue))]
        public static string SetCooldownValue = "Set Cooldown Value";

        [Localized(nameof(GlobalCooldown))]
        public static string GlobalCooldown = "Global CD";

        [Localized(nameof(RoleCooldown))]
        public static string RoleCooldown = "Role CD";

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

        [Localized(nameof(DisableFixWiring))]
        public static string DisableFixWiring = "Disable Fix Wiring";

        [Localized("DisableTaskWin")]
        public static string DisableTaskWinText = "Disable Task Win";

        [Localized(nameof(GhostSeeInfo))]
        public static string GhostSeeInfo = "Ghosts See Info";

        [Localized("LadderDeath")]
        public static string LadderDeathText = "Ladder Death";

        [Localized(nameof(ModifierTextMode))]
        public static string ModifierTextMode = "Modifier Text Mode";

        [Localized(nameof(FirstValue))]
        public static string FirstValue = "First";
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
    ResetBreaker = 32,
    FixWiring = 64
}

public enum FirstKillCooldown
{
    GlobalCooldown,
    SetCooldown,
    RoleCooldown
}

public enum ModifierTextMode
{
    First,
    Off,
    All
}