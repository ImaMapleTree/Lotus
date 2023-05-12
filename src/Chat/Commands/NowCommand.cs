using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TOHTOR.Factions;
using TOHTOR.Factions.Crew;
using TOHTOR.Factions.Impostors;
using TOHTOR.Factions.Interfaces;
using TOHTOR.Factions.Neutrals;
using TOHTOR.Managers;
using TOHTOR.Options;
using TOHTOR.Roles;
using TOHTOR.Roles.Internals;
using TOHTOR.Utilities;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;
using VentLib.Options.Game;
using VentLib.Options.IO;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Chat.Commands;

[Command("n", "now")]
public class NowCommand: ICommandReceiver
{
    private static Regex descriptionRegex = new("^.*#.*$", RegexOptions.Multiline);

    [Command("r", "role", "roles")]
    public void ListRoleOptions(PlayerControl source)
    {
        ListCrewmateOptions(source);
        ListImpostorOptions(source);
        ListNeutralKillers(source);
        ListNeutralPassive(source);
    }

    [Command("crewmates", "crewmate", "crew", "cr")]
    public void ListCrewmateOptions(PlayerControl source)
    {
        string title = FactionInstances.Crewmates.FactionColor().Colorize($"★ {FactionInstances.Crewmates.Name()} ★");
        ListRoleGroup(source, title, CustomRoleManager.AllRoles.Where(r => r.Faction is Crewmates));
    }
    [Command("impostors", "imp")]
    public void ListImpostorOptions(PlayerControl source)
    {
        string title = FactionInstances.Impostors.FactionColor().Colorize($"★ {FactionInstances.Impostors.Name()} ★");
        ListRoleGroup(source, title, CustomRoleManager.AllRoles.Where(r => r.Faction is ImpostorFaction));
    }

    [Command("nk", "neutral-killers", "nks")]
    public void ListNeutralKillers(PlayerControl source)
    {
        string title = ModConstants.Palette.KillingColor.Colorize("★ Neutral Killing ★");
        ListRoleGroup(source, title, CustomRoleManager.AllRoles.Where(r => r.SpecialType is SpecialType.NeutralKilling));
    }

    [Command("neutral", "neutrals", "np")]
    public void ListNeutralPassive(PlayerControl source)
    {
        string title = ModConstants.Palette.PassiveColor.Colorize("★ Neutrals ★");
        ListRoleGroup(source, title, CustomRoleManager.AllRoles.Where(r => r.SpecialType is SpecialType.Neutral));
    }

    public void ListNormalOptions(PlayerControl source)
    {
        string title = "";
        string content = "";
        foreach (GameOption option in GeneralOptions.AllOptions)
        {
            if (!option.IsTitle)
            {
                //content += "• " + descriptionRegex.Replace(OptionWriter.WriteAsString(option), "").Replace("*", "  •");
                content += OptionUtils.OptionText(option);
                continue;
            }

            if (title != "")
            {
                Utils.SendMessage(content[..^1], source.PlayerId, title, leftAlign: true);
            }
            
            title = $"★ {option.Name()} ★";
            content = "";
        }
        
        Utils.SendMessage(content[..^1], source.PlayerId, title, leftAlign: true);
    }

    private void ListRoleGroup(PlayerControl source, string title, IEnumerable<CustomRole> roles)
    {
        string text = roles.Where(r => r.IsEnabled()).Select(r => OptionUtils.OptionText(r.Options)).Fuse("\n");
        ChatHandler.Of(text, title).LeftAlign().Send(source);
    }


    public void Receive(PlayerControl source, CommandContext context)
    {
        if (context.Args.Length == 0) ListNormalOptions(source);
    }
}