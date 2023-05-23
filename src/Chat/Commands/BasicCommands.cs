using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Factions.Neutrals;
using Lotus.Managers;
using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Internals;
using Lotus.Utilities;
using Lotus.Extensions;
using Lotus.Managers.Friends;
using Lotus.Managers.Templates;
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
    [Localized("Color.NotInRange")] public static string ColorNotInRangeMessage = "{0} is not in range of valid colors.";
    [Localized(nameof(Winners))] public static string Winners = "Winners";
    [Localized("Dump.Success", ForceOverride = true)] public static string DumpSuccess = "Successfully dumped log. Check your logs folder for a \"dump.log!\"";
    [Localized("Ids.PlayerIdMessage")] public static string PlayerIdMessage = "{0}'s player ID is {1}";

    [Command("perc", "percentage", "percentages")]
    public static void Percentage(PlayerControl source)
    {
        string? factionName = null;
        string text = $"{HostOptionTranslations.CurrentRoles}:\n";

        OrderedDictionary<string, List<CustomRole>> rolesByFaction = new();

        string FactionName(CustomRole role)
        {
            if (role.Faction is not Solo) return role.Faction.Name();
            return role.SpecialType is SpecialType.NeutralKilling ? "Neutral Killers" : "Neutral";
        }

        CustomRoleManager.AllRoles.ForEach(r => rolesByFaction.GetOrCompute(FactionName(r), () => new List<CustomRole>()).Add(r));

        rolesByFaction.GetValues().SelectMany(s => s).ForEach(r =>
        {

            if (r.Count == 0 || r.Chance == 0) return;

            string fName = FactionName(r);
            if (factionName != fName)
            {
                text += $"\n{HostOptionTranslations.RoleCategory.Formatted(fName)}\n";
                factionName = fName;
            }


            text += $"{r.RoleName}: {r.Count} × {r.Chance}%";
            if (r.Count > 1) text += $" (+ {r.AdditionalChance}%)\n";
            else text += "\n";
        });

        ChatHandler.Of(text, HostOptionTranslations.RoleInfo).LeftAlign().Send(source);
    }

    [Command(CommandFlag.HostOnly, "dump")]
    public static void Dump(PlayerControl source)
    {
        VentLogger.SendInGame("Successfully dumped log. Check your logs folder for a \"dump.log!\"");
        VentLogger.Dump();
    }

    [Command(CommandFlag.LobbyOnly, "name")]
    public static void Name(PlayerControl source, string name)
    {
        int allowedUsers = GeneralOptions.MiscellaneousOptions.ChangeNameUsers;
        bool permitted = allowedUsers switch
        {
            0 => source.IsHost(),
            1 => source.IsHost() || PluginDataManager.FriendManager.IsFriend(source),
            2 => true,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (!permitted)
        {
            ChatHandlers.NotPermitted().Send(source);
            return;
        }

        source.RpcSetName(name);
        PluginDataManager.LastKnownAs.SetName(source.FriendCode, name);
    }

    [Command(CommandFlag.LobbyOnly, "winner", "w")]
    public static void ListWinners(PlayerControl source)
    {
        ChatHandler.Send(source, $"{Winners}: {Game.MatchData.GameHistory.LastWinners.Select(w => w.Name + $"({w.Role.RoleName})").Fuse()}");
    }

    [Command(CommandFlag.LobbyOnly, "color", "colour")]
    public static void SetColor(PlayerControl source, int color)
    {
        int allowedUsers = GeneralOptions.MiscellaneousOptions.ChangeColorAndLevelUsers;
        bool permitted = allowedUsers switch
        {
            0 => source.IsHost(),
            1 => source.IsHost() || PluginDataManager.FriendManager.IsFriend(source),
            2 => true,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (!permitted)
        {
            ChatHandlers.NotPermitted().Send(source);
            return;
        }

        if (color > Palette.PlayerColors.Length)
        {
            ChatHandler.Of($"{ColorNotInRangeMessage.Formatted(color)} (0-{Palette.PlayerColors.Length})", ModConstants.Palette.InvalidUsage.Colorize(InvalidUsage)).LeftAlign().Send(source);
            return;
        }

        source.RpcSetColor((byte)color);
    }

    private static readonly ColorGradient HostGradient = new(new Color(1f, 0.93f, 0.98f), new Color(1f, 0.57f, 0.73f));

    [Command(CommandFlag.HostOnly, "say", "s")]
    public static void Say(PlayerControl _, string message)
    {
        ChatHandler.Of(message).Title(HostGradient.Apply(HostMessage)).Send();
    }

    [Command(CommandFlag.HostOnly, "id", "ids", "pid", "pids")] // eur mom is 😣
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

    [Command("view", "v")]
    public static void View(PlayerControl source, int id) => TemplateCommands.Preview(source, id);

    [Command(CommandFlag.HostOnly, "title reload", "tload")]
    public static void ReloadTitles(PlayerControl source)
    {
        PluginDataManager.TitleManager.Reload();
        ChatHandler.Of("Successfully reloaded titles.").Send(source);
    }

    [Command("mods", "modifiers", "subroles")]
    public static void Modifiers(PlayerControl source)
    {
        ChatHandler.Of(new Template("@ModsDescriptive").Format(source), "Modifiers").LeftAlign().Send(source);
    }
}