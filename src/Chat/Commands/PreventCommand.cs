using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using Lotus.Gamemodes.Standard;
using Lotus.Managers;
using Lotus.Roles;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Utilities.Extensions;

namespace Lotus.Chat.Commands;

[Command(CommandFlag.HostOnly, "combos", "combo")]
public class PreventCommand
{
    [Command("list", "l")]
    public static void ListIllegalCombos(PlayerControl source, CommandContext _)
    {
        string illegalRoleText = IllegalRoleCombos.GetCurrentCombos().Select((li, i) => $"{i}) {GeneralExtensions.Join<string>(li)}").Join(delimiter: "\n");
        ChatHandler.Send(source, "Current Banned Combos:\n" + illegalRoleText);
    }

    [Command("ban", "add", "b")]
    public static void AddIllegalCombo(PlayerControl source, CommandContext ctx)
    {
        string input = Regex.Replace(ctx.Args.Join(delimiter: " "), "\\s*,\\s*", ",");
        List<string> roleNames = input.Split(",").ToList();
        List<string> failedRoleNames = new();
        List<CustomRole> roles = roleNames.SelectWhere(rn =>
        {
            try
            {
                return CustomRoleManager.GetRoleFromName(rn);
            }
            catch
            {
                failedRoleNames.Add(rn);
                return null;
            }
        }).ToList();

        if (failedRoleNames.Count > 0) ChatHandler.Send(source, $"Failed to add. Invalid role names: {failedRoleNames.Select(s => $"\"{s}\"").Join()}");
        else IllegalRoleCombos.AddIllegalCombo(roles);
        ChatHandler.Send(source, $"Successfully banned combo: {roleNames.Select(rn => $"\"{rn}\"").Join()}");
    }

    [Command("allow", "remove", "a")]
    public static void RemoveIllegalCombo(PlayerControl source, CommandContext _, int index)
    {
        List<string> currentCombo = IllegalRoleCombos.GetCurrentCombos()[index];
        IllegalRoleCombos.RemoveIllegalCombo(index);
        ChatHandler.Send(source, $"Successfully allowed combo: {currentCombo.Join()}");
    }
}