using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Player;
using Lotus.Managers;
using Lotus.Managers.Hotkeys;
using Lotus.Managers.Templates.Models.Backing;
using Lotus.Roles;
using Lotus.Roles.Interfaces;
using Lotus.Utilities;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Localization;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Chat.Commands.Help;

public class RoleCommand
{
    [Command("mod", "modifier", "mods")]
    public static void Modifiers(PlayerControl source)
    {
        string message = ProjectLotus.RoleManager.AllRoles.Where(r => r.RoleFlags.HasFlag(RoleFlag.IsSubrole)).OrderBy(r => r.RoleName).DistinctBy(r => r.RoleName).Select(m =>
        {
            string identifierText = m is ISubrole subrole ? m.RoleColor.Colorize(subrole.Identifier()!) + " " : "";
            return $"{identifierText}{m.RoleColor.Colorize(m.RoleName)}\n{m.Description}";
        }).Fuse("\n\n");

        SendSpecial(source, message);
    }

    [Command("r", "roles")]
    public static void Roles(PlayerControl source, CommandContext context)
    {
        Localizer localizer = Localizer.Get();
        if (context.Args.Length == 0 || context.Args[0] is "" or " ") ChatHandler.Of(TUAllRoles.GetAllRoles(true)).LeftAlign().Send(source);
        else
        {
            string roleName = context.Args.Join(delimiter: " ").ToLower().Trim().Replace("[", "").Replace("]", "");
            CustomRole? matchingRole = ProjectLotus.RoleManager.AllRoles.FirstOrDefault(r => localizer.GetAllTranslations($"Roles.{r.EnglishRoleName}.RoleName").Select(s => s.ToLowerInvariant()).Contains(roleName.ToLowerInvariant()));

            if (matchingRole == null) {
                List<CustomRole> matchingRoles = ProjectLotus.RoleManager.AllRoles.Where(r => r.RoleName.RemoveHtmlTags().ToLower().StartsWith(roleName)).ToList();
                if (matchingRoles.Count == 0) ChatHandler.Of(Localizer.Translate("Commands.Help.Roles.RoleNotFound").Formatted(roleName)).Send(source);
                else matchingRoles.ForEach(r => ShowRole(source, r));
                return;
            }

            ShowRole(source, matchingRole);
        }
    }

    private static void ShowRole(PlayerControl source, CustomRole role)
    {
        if (!PluginDataManager.TemplateManager.TryFormat(role, "help-role", out string formatted))
            formatted = $"{role.RoleName} ({role.Faction.Name()})\n{role.Blurb}\n{role.Description}\n\nOptions:\n{OptionUtils.OptionText(role.RoleOptions)}";

        SendSpecial(source, formatted);
    }

    private static void SendSpecial(PlayerControl source, string message)
    {
        if (source.IsHost() && HotkeyManager.HoldingLeftShift)
            ChatHandler.Of(message).LeftAlign().Send();
        else if (source.IsHost() && HotkeyManager.HoldingRightShift)
            Players.GetPlayers(PlayerFilter.Dead).ForEach(p => ChatHandler.Of(message).LeftAlign().Send(p));
        else ChatHandler.Of(message).LeftAlign().Send(source);
    }
}