using System.Linq;
using HarmonyLib;
using TOHTOR.Managers;
using TOHTOR.Roles;
using TOHTOR.Utilities;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;
using VentLib.Localization;
using VentLib.Localization.Attributes;

namespace TOHTOR.Chat.Commands;

[Localized("Commands.Help")]
[Command("h", "help")]
public class HelpCmd: ICommandReceiver
{
    [Command("a", "addons")]
    public static void Addons(PlayerControl source, CommandContext _)
    {
        Utils.SendMessage("Addon Info");
    }

    [Command("m", "modes")]
    public class Gamemodes
    {
        [Command("cw", "colorwars")]
        public static void ColorWars(PlayerControl source, CommandContext _) => Utils.SendMessage("Color wars info", source.PlayerId);

        [Command("nge", "nogameend")]
        public static void NoGameEnd(PlayerControl source, CommandContext _) => Utils.SendMessage("NoGameEnd Info", source.PlayerId);
    }

    [Command("r", "roles")]
    public static void Roles(PlayerControl source, CommandContext context)
    {
        Localizer localizer = Localizer.Get();
        if (context.Args.Length == 0)
            Utils.SendMessage(localizer.Translate("Commands.Help.Roles.Usage"), source.PlayerId);
        else
        {
            string roleName = context.Args.Join(delimiter: " ");
            CustomRole? matchingRole = CustomRoleManager.AllRoles.FirstOrDefault(r => localizer.GetAllTranslations($"Roles.{r.EnglishRoleName}.RoleName").Select(s => s.ToLowerInvariant()).Contains(roleName.ToLowerInvariant()));
            if (matchingRole == null) {
                Utils.SendMessage(string.Format(Localizer.Translate("Commands.Help.Roles.RoleNotFound"), roleName), source.PlayerId);
                return;
            }

            Language? language = localizer.FindLanguageFromTranslation(roleName, $"Roles.{matchingRole.EnglishRoleName}.RoleName");

            Utils.SendMessage(
                language == null ? Localizer.Translate($"Roles.{matchingRole.EnglishRoleName}.Description")
                    : language.Translate($"Roles.{matchingRole.EnglishRoleName}.Description"), source.PlayerId, leftAlign: true);
        }
    }

    // This is triggered when just using /help
    public void Receive(PlayerControl source, CommandContext context)
    {
        if (context.Args.Length > 0) return;
        string help = Localizer.Translate("Commands.Help.Alias");
        Utils.SendMessage(
                Localizer.Translate("Commands.Help.CommandList")
                + $"\n/{help} {Localizer.Translate("Commands.Help.Roles.Alias")} - {Localizer.Translate("Commands.Help.Roles.Info")}"
                + $"\n/{help} {Localizer.Translate("Commands.Help.Addons.Alias")} - {Localizer.Translate("Commands.Help.Addons.Info")}"
                + $"\n/{help} {Localizer.Translate("Commands.Help.Gamemodes.Alias")} - {Localizer.Translate("Commands.Help.Gamemodes.Info")}",
                source.PlayerId
            );
    }
}