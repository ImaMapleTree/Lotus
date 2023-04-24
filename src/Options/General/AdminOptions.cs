using System.Collections.Generic;
using TOHTOR.Extensions;
using TOHTOR.Managers;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;

namespace TOHTOR.Options.General;

[Localized("Options")]
public class AdminOptions
{
    private static Color _optionColor = CustomRoleManager.Special.GM.RoleColor;
    private static List<GameOption> additionalOptions = new();

    // ReSharper disable once InconsistentNaming
    public bool HostGM;
    public bool AutoKick;
    public bool KickPlayersWithoutFriendcodes;
    public int KickPlayersUnderLevel;
    public bool KickMobilePlayers;
    public int AutoStart;

    public bool AutoStartEnabled => AutoStart != -1;

    public AdminOptions()
    {
        new GameOptionTitleBuilder()
            .Tab(DefaultTabs.GeneralTab)
            .Title(AdminOptionTranslations.AdminTitle)
            .Color(_optionColor)
            .IsHeader(false)
            .Build();

        var hostGameMaster = Builder("HostGM")
            .Name(AdminOptionTranslations.HostGmText)
            .AddOnOffValues()
            .BindBool(b => HostGM = b)
            .IsHeader(true)
            .BuildAndRegister();

        // TODO: repeat offenders
        var autoKick = Builder("Chat AutoKick")
            .Name(AdminOptionTranslations.AutoKickText)
            .AddEnableDisabledValues()
            .BindBool(b => AutoKick = b)
            .BuildAndRegister();

        var autoKickNoFriendcode = Builder("Kick Players Without Friendcode")
            .Name(AdminOptionTranslations.AutoKickNoFriendCodeText)
            .AddEnableDisabledValues(false)
            .BindBool(b => KickPlayersWithoutFriendcodes = b)
            .BuildAndRegister();

        var autoKickUnderLevel = Builder("Kick Players Under Level")
            .Name(AdminOptionTranslations.AutoKickUnderLevel)
            .Value(v => v.Text(GeneralOptionTranslations.DisabledText).Value(0).Color(Color.red).Build())
            .AddIntRange(1, 100, 1)
            .BindInt(i => KickPlayersUnderLevel = i)
            .BuildAndRegister();

        var autoKickMobile = Builder("Kick Mobile Players")
            .Name(AdminOptionTranslations.AutoKickMobile)
            .AddEnableDisabledValues(false)
            .BindBool(b => KickMobilePlayers = b)
            .BuildAndRegister();

        var autoStart = Builder("AutoStart")
            .Name(AdminOptionTranslations.AutoStartText)
            .Value(v => v.Text(GeneralOptionTranslations.DisabledText).Value(-1).Color(Color.red).Build())
            .AddIntRange(5, 15, suffix: " " + AdminOptionTranslations.AutoStartSuffix)
            .BindInt(i => AutoStart = i)
            .BuildAndRegister();

        additionalOptions.ForEach(o => o.Register());
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
        public static string AdminTitle = "Admin Options";

        [Localized("HostGM")]
        public static string HostGmText = "Host GM";

        [Localized("AutoKick")]
        public static string AutoKickText = "Chat AutoKick";

        [Localized("AutoKickNoFriendcode")]
        public static string AutoKickNoFriendCodeText = "Kick Players w/o Friendcodes";

        [Localized("AutoKickLevel")]
        public static string AutoKickUnderLevel = "Kick Players Under Level";

        [Localized(nameof(AutoKickMobile))]
        public static string AutoKickMobile = "Kick Mobile Players";

        // Auto Start
        [Localized("AutoStart")]
        public static string AutoStartText = "AutoStart";
        [Localized("AutoStartOptionSuffix")]
        public static string AutoStartSuffix = "Players";
    }

}

public enum BannedMobileDevice
{

}