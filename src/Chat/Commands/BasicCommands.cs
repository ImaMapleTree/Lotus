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
    [Localized("Dump.Success")] public static string DumpSuccess = "Successfully dumped log to: {0}";
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

        Utils.SendMessage(text, source.PlayerId, HostOptionTranslations.RoleInfo, true);
    }

    [Command(CommandFlag.HostOnly, "dump")]
    public static void Dump(PlayerControl source)
    {
        ChatHandler.Of("Successfully dumped logs.").Send(source);
        /*Utils.SendMessage(DumpSuccess.Formatted(new FileInfo(VentLogger.Dump()!).Directory!.GetFile("dump.log").FullName), source.PlayerId);*/
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
            Utils.SendMessage(NotPermittedText, source.PlayerId, ModConstants.Palette.InvalidUsage.Colorize(NotPermittedTitle), true);
            return;
        }

        source.RpcSetName(name);
        PluginDataManager.LastKnownAs.SetName(source.FriendCode, name);
    }

    [Command(CommandFlag.LobbyOnly, "winner", "w")]
    public static void ListWinners(PlayerControl source)
    {
        Utils.SendMessage($"{Winners}: {Game.MatchData.GameHistory.LastWinners.Select(w => w.Name + $"({w.Role.RoleName})").Fuse()}", source.PlayerId);
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
            Utils.SendMessage(NotPermittedText, source.PlayerId, ModConstants.Palette.InvalidUsage.Colorize(NotPermittedTitle), true);
            return;
        }

        if (color > Palette.PlayerColors.Length)
        {
            Utils.SendMessage($"{ColorNotInRangeMessage.Formatted(color)} (0-{Palette.PlayerColors.Length})", source.PlayerId, ModConstants.Palette.InvalidUsage.Colorize(InvalidUsage), true);
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
            Utils.SendMessage(playerIds, source.PlayerId, leftAlign: true);
            return;
        }

        string name = context.Join();
        PlayerControl? player = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.name == name);
        Utils.SendMessage(player == null ? PlayerNotFoundText.Formatted(name) : PlayerIdMessage.Formatted(name, player.PlayerId), source.PlayerId, leftAlign: true);
    }

    [Command("view", "v")]
    public static void View(PlayerControl source, int id) => TemplateCommands.Preview(source, id);

    [Command(CommandFlag.HostOnly, "title reload", "tload")]
    public static void ReloadTitles(PlayerControl source)
    {
        PluginDataManager.TitleManager.Reload();
        ChatHandler.Of("Successfully reloaded titles.").Send(source);
    }
}