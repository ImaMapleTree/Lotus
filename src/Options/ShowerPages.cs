using System;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.Logging;
using Lotus.Roles;
using Lotus.Roles.Builtins;
using Lotus.Roles.Internals.Enums;
using Lotus.Utilities;
using LotusTrigger.Options;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Options;

[Localized("OptionShower")]
public class ShowerPages
{
    [Localized("ActiveRolesList")]
    private static string ActiveRolesList = "Active Roles List";

    public static void InitPages()
    {
        OptionShower shower = OptionShower.GetOptionShower();
        shower.AddPage(VanillaPage());
        shower.AddPage(EnabledRolePage());
        shower.AddPage(RoleOptionsPage());
        shower.AddPage(EnableGeneralPage());
    }

    private static Func<string> VanillaPage()
    {
        return () => GameOptionsManager.Instance.CurrentGameOptions.ToHudString(GameData.Instance
            ? GameData.Instance.PlayerCount
            : 10) + "\n";
    }

    private static Func<string> EnabledRolePage()
    {
        return () =>
        {
            DevLogger.Log("Enabled Role Page");
            string content = $"Gamemode: {Game.CurrentGamemode.Name}\n\n";
            content += $"{GameMaster.GMColor.Colorize("GM")}: {Utils.GetOnOffColored(GeneralOptions.AdminOptions.HostGM)}\n\n";
            content += ActiveRolesList + "\n";

            ProjectLotus.RoleManager.Not(LotusRoleType.Internals).Where(role => role.IsEnabled()).ForEach(role =>
            {
                Color color = role.RoleColor;
                content += $"{color.Colorize(role.RoleName)}: {role.Chance}% x {role.Count}\n";
            });
            return content;
        };
    }

    public static Func<string> RoleOptionsPage()
    {
        return () =>
        {
            DevLogger.Log("Role Option Page");
            var content = "";
            ProjectLotus.RoleManager.Not(LotusRoleType.Internals).Where(role => role.IsEnabled()).ForEach(role =>
            {
                var opt = role.RoleOptions;
                content += $"{opt.Name()}: {opt.GetValueText()}\n";
                if (opt.Children.Matches(opt.GetValue()))
                    content = ShowChildren(opt, opt.Color, content);
                content += "\n";
            });
            return content;
        };
    }

    private static Func<string> EnableGeneralPage()
    {
        return () =>
        {
            string optionString = "";
            var optionManager = OptionManager.GetManager();

            optionManager.GetOptions().Where(opt => opt.GetType() == typeof(GameOption)).Cast<GameOption>().Do(opt =>
            {
                CustomRole? matchingRole = ProjectLotus.RoleManager.AllRoles.FirstOrDefault(r => r.RoleOptions == opt);

                if (matchingRole != null) return;

                optionString += $"{opt.Name()}: {opt.GetValueText()}\n";
                if (opt.Children.Matches(opt.GetValue()))
                    optionString = ShowChildren(opt, opt.Color, optionString);
                optionString += "\n";
            });

            return optionString;
        };
    }

    private static string ShowChildren(GameOption option, Color color, string text)
    {

        option.Children.Cast<GameOption>().ForEach((opt, index) =>
        {
            if (opt.Name() == "Maximum") return;
            text += color.Colorize("┃".Repeat(option.Level - 2));
            text += color.Colorize(index == option.Children.Count - 1 ? "┗ " : "┣ ");
            text += $"{opt.Name()}: {opt.GetValueText()}\n";
            if (opt.Children.Matches(opt.GetValue())) text = ShowChildren(opt, color, text);
        });
        return text;
    }
}