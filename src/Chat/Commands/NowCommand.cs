using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lotus.Factions;
using Lotus.Factions.Crew;
using Lotus.Factions.Impostors;
using Lotus.Roles;
using Lotus.Roles.Internals.Enums;
using LotusTrigger.Options;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Chat.Commands;

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
        ListModifiers(source);
    }

    [Command("crewmates", "crewmate", "crew", "cr")]
    public void ListCrewmateOptions(PlayerControl source)
    {
        string title = FactionInstances.Crewmates.Color.Colorize($"★ {FactionInstances.Crewmates.Name()} ★");
        ListRoleGroup(source, title, ProjectLotus.RoleManager.AllRoles.Where(r => r.Faction is Crewmates));
    }
    [Command("impostors", "imp")]
    public void ListImpostorOptions(PlayerControl source)
    {
        string title = FactionInstances.Impostors.Color.Colorize($"★ {FactionInstances.Impostors.Name()} ★");
        ListRoleGroup(source, title, ProjectLotus.RoleManager.AllRoles.Where(r => r.Faction is ImpostorFaction));
    }

    [Command("nk", "neutral-killers", "nks")]
    public void ListNeutralKillers(PlayerControl source)
    {
        string title = ModConstants.Palette.KillingColor.Colorize("★ Neutral Killing ★");
        ListRoleGroup(source, title, ProjectLotus.RoleManager.AllRoles.Where(r => r.SpecialType is SpecialType.NeutralKilling));
    }

    [Command("neutral", "neutrals", "np")]
    public void ListNeutralPassive(PlayerControl source)
    {
        string title = ModConstants.Palette.PassiveColor.Colorize("★ Neutrals ★");
        ListRoleGroup(source, title, ProjectLotus.RoleManager.AllRoles.Where(r => r.SpecialType is SpecialType.Neutral));
    }

    [Command("mods", "mod", "subroles", "modifiers", "modifier")]
    public void ListModifiers(PlayerControl source)
    {
        string title = ModConstants.Palette.ModifierColor.Colorize("★ Modifiers ★");
        ListRoleGroup(source, title, ProjectLotus.RoleManager.All(LotusRoleType.Modifiers));
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

            if (title != "") ChatHandler.Of(content[..^1], title).LeftAlign().Send(source);

            title = $"★ {option.Name()} ★";
            content = "";
        }

        ChatHandler.Of(content[..^1], title).LeftAlign().Send(source);
    }

    private void ListRoleGroup(PlayerControl source, string title, IEnumerable<CustomRole> roles)
    {
        string text = roles.Where(r => r.IsEnabled()).Select(r => OptionUtils.OptionText(r.RoleOptions)).Fuse("\n");
        ChatHandler.Of(text, title).LeftAlign().Send(source);
    }


    public void Receive(PlayerControl source, CommandContext context)
    {
        if (context.Args.Length == 0) ListNormalOptions(source);
    }
}