using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.Managers;
using Lotus.Managers.Hotkeys;
using Lotus.Roles;
using Lotus.Utilities;
using Lotus.Managers.Templates;
using Lotus.Roles.Subroles;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;
using VentLib.Localization;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Chat.Commands;

[Localized("Commands.Help")]
[Command("h", "help")]
public class HelpCmd: ICommandReceiver
{
    static HelpCmd()
    {
        PluginDataManager.TemplateManager.RegisterTag("help-role",
            "This tag is for the message shown when players use /h r. By default there is no template set for this tag, and the game uses a built-in formatting. But you may utilize this tag if you'd like to customize how this help is shown to the player.\n(<i>This tag utilizes ^Role_XXX variables.</i>)");
    }

    [Command("a", "addons")]
    public static void Addons(PlayerControl source, CommandContext _)
    {
        ChatHandler.Send(source, "Addon Info");
    }

    [Command("m", "modes")]
    public class Gamemodes
    {
        [Command("cw", "colorwars")]
        public static void ColorWars(PlayerControl source, CommandContext _) => ChatHandler.Send(source, "Color wars info");

        [Command("nge", "nogameend")]
        public static void NoGameEnd(PlayerControl source, CommandContext _) => ChatHandler.Send(source, "No game info");
    }

    [Command("r", "roles")]
    public static void Roles(PlayerControl source, CommandContext context)
    {
        Localizer localizer = Localizer.Get();
        if (context.Args.Length == 0) ChatHandlers.InvalidCmdUsage("Correct usage: /h r [role]").Send(source);
        else
        {
            string roleName = context.Args.Join(delimiter: " ").ToLower().Trim().Replace("[", "").Replace("]", "");
            CustomRole? matchingRole = CustomRoleManager.AllRoles.FirstOrDefault(r => localizer.GetAllTranslations($"Roles.{r.EnglishRoleName}.RoleName").Select(s => s.ToLowerInvariant()).Contains(roleName.ToLowerInvariant()));
            if (matchingRole == null) {
                List<CustomRole> matchingRoles = CustomRoleManager.AllRoles.Where(r => r.RoleName.RemoveHtmlTags().ToLower().StartsWith(roleName)).ToList();
                if (matchingRoles.Count == 0) ChatHandler.Of(Localizer.Translate("Commands.Help.Roles.RoleNotFound").Formatted(roleName)).Send(source);
                else matchingRoles.ForEach(r => ShowRole(source, r));
                return;
            }

            ShowRole(source, matchingRole);
        }
    }

    private static void ShowRole(PlayerControl source, CustomRole role)
    {
        if (!PluginDataManager.TemplateManager.TryFormat(source, role, "help-role", out string formatted))
            formatted = $"{role.RoleName} ({role.Faction.Name()})\n{role.Blurb}\n{role.Description}\n\nOptions:\n{OptionUtils.OptionText(role.RoleOptions)}";

        SendSpecial(source, formatted);
    }

    [Command("mod", "modifier", "mods")]
    public static void Modifiers(PlayerControl source)
    {
        string message = CustomRoleManager.ModifierRoles.Select(m =>
        {
            string identifierText = m is Subrole subrole ? m.RoleColor.Colorize(subrole.Identifier()!) + " " : "";
            return $"{identifierText}{m.RoleColor.Colorize(m.RoleName)}\n{m.Description}";
        }).Fuse("\n\n");

        SendSpecial(source, message);
    }

    private static void SendSpecial(PlayerControl source, string message)
    {
        if (source.IsHost() && HotkeyManager.HoldingLeftShift)
            ChatHandler.Of(message).LeftAlign().Send();
        else if (source.IsHost() && HotkeyManager.HoldingRightShift)
            Game.GetDeadPlayers().ForEach(p => ChatHandler.Of(message).LeftAlign().Send(p));
        else {
            ChatHandler.Of(message).LeftAlign().Send(source);
        }
    }

    // This is triggered when just using /help
    public void Receive(PlayerControl source, CommandContext context)
    {
        if (context.Args.Length > 0) return;
        string help = Localizer.Translate("Commands.Help.Alias");
        ChatHandler.Send( source,
                Localizer.Translate("Commands.Help.CommandList")
                + $"\n/{help} {Localizer.Translate("Commands.Help.Roles.Alias")} - {Localizer.Translate("Commands.Help.Roles.Info")}"
                + $"\n/{help} {Localizer.Translate("Commands.Help.Addons.Alias")} - {Localizer.Translate("Commands.Help.Addons.Info")}"
                + $"\n/{help} {Localizer.Translate("Commands.Help.Gamemodes.Alias")} - {Localizer.Translate("Commands.Help.Gamemodes.Info")}"
        );
    }
}