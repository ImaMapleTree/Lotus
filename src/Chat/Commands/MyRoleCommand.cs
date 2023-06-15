using System.Linq;
using Lotus.Roles;
using Lotus.Extensions;
using Lotus.Managers;
using Lotus.Managers.Templates.Models;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Chat.Commands;

public class MyRoleCommand
{
    private static int _previousLevel;

    public static string GenerateMyRoleText(CustomRole role)
    {
        string output = $"{role.RoleColor.Colorize(role.RoleName)} ({role.Faction.Color.Colorize(role.Faction.Name())}):";
        output += $"\n{role.Description}";
        return output;
    }

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

    private static void ShowRoleDescription(PlayerControl source)
    {
        CustomRole role = source.GetCustomRole();
        string output = GenerateMyRoleText(role);
        ChatHandler.Of(output).LeftAlign().Send(source);
        if (!source.GetSubroles().IsEmpty()) ChatHandler.Of(new Template("${ModsDescriptive}").Format(source), "Modifiers").LeftAlign().Send(source);
    }

    [Command(CommandFlag.InGameOnly, "desc", "description")]
    private static void ShowFirstMeetingText(PlayerControl source)
    {
        PluginDataManager.TemplateManager.GetTemplates("meeting-first")?.ForEach(t => t.SendMessage(source, source));
    }

    [Command(CommandFlag.InGameOnly, "o", "option", "options")]
    private static void ShowRoleOptions(PlayerControl source)
    {
        CustomRole role = source.GetCustomRole();
        string output = $"{role.RoleColor.Colorize(role.RoleName)} ({role.Faction.Color.Colorize(role.Faction.Name())}):\n";

        output += OptionUtils.OptionText(role.RoleOptions);

        if (!source.GetSubroles().IsEmpty()) output += "\n";
        output += source.GetSubroles().Select(sr => $"{sr.ColoredRoleName()}\n{OptionUtils.OptionText(sr.RoleOptions)}").Fuse("\n");

        ChatHandler.Of(output).LeftAlign().Send(source);
    }
}