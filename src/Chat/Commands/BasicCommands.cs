using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Chat.Patches;
using Lotus.Factions.Neutrals;
using Lotus.Managers;
using Lotus.Roles;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals.Enums;
using UnityEngine;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Localization.Attributes;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Chat.Commands;

[Localized("Commands")]
public class BasicCommands: CommandTranslations
{
    [Localized(nameof(Winners))] public static string Winners = "Winners";
    [Localized("Dump.Success")] public static string DumpSuccess = "Successfully dumped log. Check your logs folder for a \"dump.log!\"";
    [Localized("Ids.PlayerIdMessage")] public static string PlayerIdMessage = "{0}'s player ID is {1}";

    [Command("perc", "percentage", "percentages", "p")]
    public static void Percentage(PlayerControl source)
    {
        string? factionName = null;
        string text = $"{HostOptionTranslations.CurrentRoles}:\n";

        OrderedDictionary<string, List<CustomRole>> rolesByFaction = new();

        string FactionName(CustomRole role)
        {
            if (role is ISubrole) return "Modifiers";
            if (role.Faction is not Neutral) return role.Faction.Name();
            return role.SpecialType is SpecialType.NeutralKilling ? "Neutral Killers" : "Neutral";
        }

        ProjectLotus.RoleManager.AllRoles.ForEach(r => rolesByFaction.GetOrCompute(FactionName(r), () => new List<CustomRole>()).Add(r));

        rolesByFaction.GetValues().SelectMany(s => s).ForEach(r =>
        {

            if (r.Count == 0 || r.Chance == 0) return;

            string fName = FactionName(r);
            if (factionName != fName)
            {
                if (factionName == "Modifiers") text += $"\n★ {factionName}\n";
                else text += $"\n{HostOptionTranslations.RoleCategory.Formatted(fName)}\n";
                factionName = fName;
            }


            text += $"{r.RoleName}: {r.Count} × {r.Chance}%";
            if (r.Count > 1) text += $" (+ {r.AdditionalChance}%)\n";
            else text += "\n";
        });

        ChatHandler.Of(text, HostOptionTranslations.RoleInfo).LeftAlign().Send(source);
    }

    [Command(CommandFlag.HostOnly, "dump")]
    public static void Dump(PlayerControl _)
    {
        VentLogger.SendInGame("Successfully dumped log. Check your logs folder for a \"dump.log!\"");
        VentLogger.Dump();

        OnChatPatch.EatMessage = true;
    }

    [Command(CommandFlag.LobbyOnly, "winner", "w")]
    public static void ListWinners(PlayerControl source)
    {
        if (Game.MatchData.GameHistory.LastWinners == null!) new ChatHandler()
            .Title(t => t.Text(CommandError).Color(ModConstants.Palette.KillingColor).Build())
            .LeftAlign()
            .Message(NoPreviousGameText)
            .Send(source);
        else
        {
            string winnerText = Game.MatchData.GameHistory.LastWinners.Select(w => $"• {w.Name} ({w.Role.RoleName})").Fuse("\n");
            ChatHandler.Of(winnerText, ModConstants.Palette.WinnerColor.Colorize(Winners)).LeftAlign().Send(source);
        }

    }

    private static readonly ColorGradient HostGradient = new(new Color(1f, 0.93f, 0.98f), new Color(1f, 0.57f, 0.73f));

    [Command(CommandFlag.HostOnly, "say", "s")]
    public static void Say(PlayerControl _, string message)
    {
        ChatHandler.Of(message).Title(HostGradient.Apply(HostMessage)).Send();
    }

    [Command("id", "ids", "pid", "pids")] // eur mom is 😣
    public static void PlayerIds(PlayerControl source, CommandContext context)
    {
        if (context.Args.Length == 0)
        {
            string playerIds = "★ Player IDs ★\n-=-=-=-=-=-=-=-=-\n";
            playerIds += PlayerControl.AllPlayerControls.ToArray().Select(p => $"{p.PlayerId} - {p.name} ({ModConstants.ColorNames[p.cosmetics.ColorId]})").Fuse("\n");
            ChatHandler.Of(playerIds).LeftAlign().Send(source);
            return;
        }

        string name = context.Join();
        PlayerControl? player = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.name == name);
        ChatHandler.Of(player == null ? PlayerNotFoundText.Formatted(name) : PlayerIdMessage.Formatted(name, player.PlayerId)).LeftAlign().Send(source);
    }

    [Command(CommandFlag.HostOnly, "tload")]
    public static void ReloadTitles(PlayerControl source)
    {
        OnChatPatch.EatMessage = true;
        PluginDataManager.TitleManager.Reload();
        ChatHandler.Of("Successfully reloaded titles.").Send(source);
    }

    [Command(CommandFlag.HostOnly | CommandFlag.InGameOnly, "fix")]
    public static void FixPlayer(PlayerControl source, CommandContext context, byte id)
    {
        if (context.Args.Length == 0)
        {
            PlayerIds(source, context);
            return;
        }

        PlayerControl? player = Players.FindPlayerById(id);
        if (player == null) ChatHandler.Of(PlayerNotFoundText.Formatted(id), CommandError).LeftAlign().Send(source);
        else if (!BlackscreenResolver.PerformForcedReset(player)) ChatHandler.Of("Unable to perform forced Blackscreen Fix. No players have died yet.", CommandError).LeftAlign().Send(source);
        else ChatHandler.Of($"Successfully cleared blackscreen of \"{player.name}\"").LeftAlign().Send(source);
    }
}