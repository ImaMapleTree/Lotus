using System.Linq;
using Lotus.Roles;
using Lotus.Utilities;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;
using VentLib.Options;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace Lotus.Chat.Commands;

public class RoleInfoCommand
{
    private static int _previousLevel = 0;
    
    [Command(CommandFlag.InGameOnly, "m", "myrole")]
    public static void MyRole(PlayerControl source, CommandContext context)
    {
        if (context.Args.Length == 0) {
            ShowRoleDescription(source);
            return;
        }
        string pageString = context.Args[0];
        if (!int.TryParse(pageString, out int page) || page <= 1) ShowRoleDescription(source);
        else ShowRoleOptions(source);
    }

    
    [Command(CommandFlag.InGameOnly, "desc", "description")]
    private static void ShowRoleDescription(PlayerControl source)
    {
        CustomRole role = source.GetCustomRole();
        string output = $"{role.RoleColor.Colorize(role.RoleName)} ({role.Faction.FactionColor().Colorize(role.Faction.Name())}):";
        output += $"\n{role.Description}";
        Utils.SendMessage(output, source.PlayerId, leftAlign: true);
    }

    [Command(CommandFlag.InGameOnly, "o", "option", "options")]
    private static void ShowRoleOptions(PlayerControl source)
    {
        CustomRole role = source.GetCustomRole();
        string output = $"{role.RoleColor.Colorize(role.RoleName)} ({role.Faction.FactionColor().Colorize(role.Faction.Name())}):";

        Option? optionMatch = OptionManager.GetManager(file: "role_options.txt").GetOptions().FirstOrDefault(h => h.Name().RemoveHtmlTags() == role.RoleName);
        if (optionMatch == null) { ShowRoleDescription(source); return; }

        foreach (var child in optionMatch.Children) UpdateOutput(ref output, child);

        ChatHandler.Of(output).LeftAlign().Send(source);
    }

    private static void UpdateOutput(ref string output, Option options)
    {
        if (options is not GameOption gameOption) return;
        if (gameOption.Level < _previousLevel)
            output += "\n";
        _previousLevel = gameOption.Level;
        string valueText = gameOption.Color == Color.white ? gameOption.GetValueText() : gameOption.Color.Colorize(gameOption.GetValueText());
        output += $"\n{gameOption.Name()} => {valueText}";

    }
}