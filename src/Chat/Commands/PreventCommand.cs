using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using TOHTOR.Gamemodes.Standard;
using TOHTOR.Managers;
using TOHTOR.Roles;
using TOHTOR.Utilities;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Chat.Commands;

[Command(CommandFlag.HostOnly, "combos", "combo")]
public class PreventCommand
{
    [Command("list", "l")]
    public static void ListIllegalCombos(PlayerControl source, CommandContext _)
    {
        string illegalRoleText = IllegalRoleCombos.GetCurrentCombos().Select((li, i) => $"{i}) {GeneralExtensions.Join<string>(li)}").Join(delimiter: "\n");
        Utils.SendMessage("Current Banned Combos:\n" + illegalRoleText, source.PlayerId);
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

        if (failedRoleNames.Count > 0) Utils.SendMessage($"Failed to add. Invalid role names: {failedRoleNames.Select(s => $"\"{s}\"").Join()}", source.PlayerId);
        else IllegalRoleCombos.AddIllegalCombo(roles);
        Utils.SendMessage($"Successfully banned combo: {roleNames.Select(rn => $"\"{rn}\"").Join()}", source.PlayerId);
    }

    [Command("allow", "remove", "a")]
    public static void RemoveIllegalCombo(PlayerControl source, CommandContext _, int index)
    {
        List<string> currentCombo = IllegalRoleCombos.GetCurrentCombos()[index];
        IllegalRoleCombos.RemoveIllegalCombo(index);
        Utils.SendMessage($"Successfully allowed combo: {currentCombo.Join()}", source.PlayerId);
    }
}