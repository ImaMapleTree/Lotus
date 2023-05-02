using System;
using System.Collections.Generic;
using System.Linq;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Factions.Neutrals;
using TOHTOR.Managers;
using TOHTOR.Options;
using TOHTOR.Roles;
using TOHTOR.Roles.Internals;
using TOHTOR.Utilities;
using UnityEngine;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Localization.Attributes;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Chat.Commands;

[Localized("Commands")]
public class BasicCommands: CommandTranslations
{
    [Localized("Color.NotInRange")] public static string ColorNotInRangeMessage = "{0} is not in range of valid colors.";
    [Localized(nameof(Winners))] public static string Winners = "Winners";
    [Localized("Dump.Success")] public static string DumpSuccess = "Successfully dumped log to: {0}";

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
        Utils.SendMessage(DumpSuccess.Formatted(VentLogger.Dump()), source.PlayerId);
    }

    [Command(CommandFlag.LobbyOnly, "name")]
    public static void Name(PlayerControl source, string name)
    {
        int allowedUsers = GeneralOptions.MiscellaneousOptions.ChangeNameUsers;
        bool permitted = (allowedUsers) switch
        {
            0 => source.IsHost(),
            1 => source.IsHost() || FriendUtils.IsFriend(source),
            2 => true,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (!permitted)
        {
            Utils.SendMessage(NotPermittedText, source.PlayerId, InvalidColor.Colorize(NotPermittedTitle), true);
            return;
        }

        source.RpcSetName(name);
    }

    [Command(CommandFlag.LobbyOnly, "winner", "w")]
    public static void ListWinners(PlayerControl source)
    {
        Utils.SendMessage($"{Winners}: {Game.GameHistory.LastWinners.Select(w => w.Name + $"({w.Role.RoleName})").Fuse()}", source.PlayerId);
    }

    [Command(CommandFlag.LobbyOnly, "color", "colour")]
    public static void SetColor(PlayerControl source, int color)
    {
        int allowedUsers = GeneralOptions.MiscellaneousOptions.ChangeColorAndLevelUsers;
        bool permitted = (allowedUsers) switch
        {
            0 => source.IsHost(),
            1 => source.IsHost() || FriendUtils.IsFriend(source),
            2 => true,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (!permitted)
        {
            Utils.SendMessage(NotPermittedText, source.PlayerId, InvalidColor.Colorize(NotPermittedTitle), true);
            return;
        }

        if (color > Palette.PlayerColors.Length)
        {
            Utils.SendMessage($"{ColorNotInRangeMessage.Formatted(color)} (0-{Palette.PlayerColors.Length})", source.PlayerId, InvalidColor.Colorize(InvalidUsage), true);
            return;
        }

        source.RpcSetColor((byte)color);
    }

    private static readonly ColorGradient HostGradient = new(new Color(1f, 0.93f, 0.98f), new Color(1f, 0.57f, 0.73f));

    [Command(CommandFlag.HostOnly, "say", "s")]
    public static void Say(PlayerControl _, string message)
    {
        Utils.SendMessage(message, title: HostGradient.Apply(HostMessage));
    }
}