using System.Collections.Generic;
using Lotus;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.Options;
using Lotus.Patches.Network;
using Lotus.Roles.Builtins;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Options.IO;

namespace LotusTrigger.Options.General;

[Localized(ModConstants.Options)]
public class AdminOptions
{
    private static Color _optionColor = GameMaster.GMColor;
    private static List<GameOption> additionalOptions = new();

    // ReSharper disable once InconsistentNaming
    public bool HostGM;
    public bool AutoKick;
    public bool KickPlayersWithoutFriendcodes;
    public int KickPlayersUnderLevel;
    public bool KickMobilePlayers;

    public int AutoStartPlayerThreshold;
    public int AutoStartMaxTime = -1;
    public int AutoStartGameCountdown;
    public bool AutoPlayAgain;

    public Cooldown AutoCooldown = new();
    public bool AutoStartEnabled;
    public List<GameOption> AllOptions = new();

    public AdminOptions()
    {
        AllOptions.Add(new GameOptionTitleBuilder()
            .Tab(DefaultTabs.GeneralTab)
            .Title(AdminOptionTranslations.AdminTitle)
            .Color(_optionColor)
            .IsHeader(false)
            .Build());

        AllOptions.Add(Builder("HostGM")
            .Name(AdminOptionTranslations.HostGmText)
            .AddOnOffValues(false)
            .BindBool(b => HostGM = b)
            .IsHeader(true)
            .BuildAndRegister());

        // TODO: repeat offenders
        AllOptions.Add(Builder("Chat AutoKick")
            .Name(AdminOptionTranslations.AutoKickText)
            .AddEnableDisabledValues()
            .BindBool(b => AutoKick = b)
            .BuildAndRegister());

        AllOptions.Add(Builder("Kick Players Without Friendcode")
            .Name(AdminOptionTranslations.AutoKickNoFriendCodeText)
            .AddEnableDisabledValues(false)
            .BindBool(b => KickPlayersWithoutFriendcodes = b)
            .BuildAndRegister());

        AllOptions.Add(Builder("Kick Players Under Level")
            .Name(AdminOptionTranslations.AutoKickUnderLevel)
            .Value(v => v.Text(GeneralOptionTranslations.DisabledText).Value(0).Color(Color.red).Build())
            .AddIntRange(1, 100, 1)
            .BindInt(i => KickPlayersUnderLevel = i)
            .BuildAndRegister());

        AllOptions.Add(Builder("Kick Mobile Players")
            .Name(AdminOptionTranslations.AutoKickMobile)
            .AddEnableDisabledValues(false)
            .BindBool(b => KickMobilePlayers = b)
            .BuildAndRegister());

        AllOptions.Add(Builder("Auto Start")
            .Name(AdminOptionTranslations.AutoStartText)
            .AddOnOffValues(false)
            .BindBool(b =>
            {
                AutoStartEnabled = b;
                if (GameStartManager.Instance != null && !b) GameStartManager.Instance.ResetStartState();
            })
            .ShowSubOptionPredicate(b => (bool)b)
            .SubOption(sub2 => sub2
                .KeyName("Player Threshold", AdminOptionTranslations.AutoStartPlayerThreshold)
                .Value(v => v.Text(GeneralOptionTranslations.OffText).Value(-1).Color(Color.red).Build())
                .AddIntRange(5, 15, suffix: " " + AdminOptionTranslations.AutoStartSuffix)
                .IOSettings(io => io.UnknownValueAction = ADEAnswer.Allow)
                .BindInt(i => AutoStartPlayerThreshold = i)
                .Build())
            .SubOption(sub2 => sub2
                .KeyName("Maximum Wait Time", AdminOptionTranslations.AutoStartMaxWaitTime)
                .Value(v => v.Text(GeneralOptionTranslations.OffText).Value(-1).Color(Color.red).Build())
                .AddIntRange(30, 540, 15, 0, GeneralOptionTranslations.SecondsSuffix)
                .IOSettings(io => io.UnknownValueAction = ADEAnswer.Allow)
                .BindInt(i =>
                {
                    if (LobbyBehaviour.Instance == null) return;
                    AutoStartMaxTime = i;
                    if (i == -1) AutoCooldown.Finish();
                    else
                    {
                        AutoCooldown.SetDuration(i);
                        AutoCooldown.Start();
                    }
                    PlayerJoinPatch.CheckAutostart();
                })
                .Build())
            .SubOption(sub2 => sub2
                .KeyName("Game Countdown", AdminOptionTranslations.AutoStartGameCountdown)
                .AddIntRange(0, 20, 2, 5, GeneralOptionTranslations.SecondsSuffix)
                .BindInt(i => AutoStartGameCountdown = i)
                .Build())
            .BuildAndRegister());

        AllOptions.Add(Builder("Auto Play Again")
            .Name(AdminOptionTranslations.AutoPlayAgain)
            .AddEnableDisabledValues()
            .BindBool(b => AutoPlayAgain = b)
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

    private GameOptionBuilder Builder(string key) => new GameOptionBuilder().Key(key).Tab(DefaultTabs.GeneralTab).Color(_optionColor);

    [Localized("Admin")]
    private class AdminOptionTranslations
    {
        [Localized("SectionTitle")]
        public static string AdminTitle = "Host Options";

        [Localized("HostGM")]
        public static string HostGmText = "Host GM";

        [Localized("AutoKick")]
        public static string AutoKickText = "Chat Auto Kick";

        [Localized("AutoKickNoFriendcode")]
        public static string AutoKickNoFriendCodeText = "Kick Players w/o Friendcodes";

        [Localized("AutoKickLevel")]
        public static string AutoKickUnderLevel = "Kick Players Under Level";

        [Localized(nameof(AutoKickMobile))]
        public static string AutoKickMobile = "Kick Mobile Players";

        // Auto Start
        [Localized("AutoStart")]
        public static string AutoStartText = "Auto Start";

        [Localized(nameof(AutoStartPlayerThreshold))]
        public static string AutoStartPlayerThreshold = "Player Threshold";

        [Localized(nameof(AutoStartMaxWaitTime))]
        public static string AutoStartMaxWaitTime = "Maximum Wait Time";

        [Localized(nameof(AutoStartGameCountdown))]
        public static string AutoStartGameCountdown = "Game Countdown";

        [Localized("AutoStartOptionSuffix")]
        public static string AutoStartSuffix = "Players";

        [Localized(nameof(AutoPlayAgain))]
        public static string AutoPlayAgain = "Auto Play Again";
    }

}

public enum BannedMobileDevice
{

}