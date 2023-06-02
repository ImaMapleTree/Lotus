using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.Logging;
using Lotus.Managers.History;
using Lotus.Options;
using Lotus.Victory.Conditions;
using Microsoft.VisualBasic;
using UnityEngine;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using static Lotus.ModConstants.Palette;

namespace Lotus.Chat.Commands;

[LoadStatic]
public class LastResultCommand: CommandTranslations
{
    static LastResultCommand()
    {
        Hooks.NetworkHooks.ReceiveVersionHook.Bind(nameof(LastResultCommand), versionEvent =>
        {
            if (!GeneralOptions.MiscellaneousOptions.AutoDisplayResults) return;
            if (versionEvent.Player == null) return;
            if (PlayerHistories == null) return;
            if (AmongUsClient.Instance.NetworkMode is not NetworkModes.LocalGame && PlayerHistories.All(p => p.UniquePlayerId.ToFriendcode() != versionEvent.Player.FriendCode)) return;
            Async.Schedule(() => GeneralResults(versionEvent.Player), 0.5f);
        });
        Hooks.NetworkHooks.GameJoinHook.Bind(nameof(LastResultCommand), gameJoinEvent =>
        {
            if (!GeneralOptions.MiscellaneousOptions.AutoDisplayResults) return;
            if (gameJoinEvent.IsNewLobby) return;
            Async.WaitUntil(() => PlayerControl.LocalPlayer, p => p != null, GeneralResults, 0.1f, 30);
        });
    }

    [Command(CommandFlag.LobbyOnly, "last", "l")]
    public static void LastGame(PlayerControl source, CommandContext context)
    {
        if (PlayerHistories == null) ErrorHandler(source).Message(LRTranslations.NoPreviousGameText).Send();
        else if (context.Args.Length == 0) GeneralResults(source);
        else PlayerResults(source, context.Join());
    }

    public static void PlayerResults(PlayerControl source, string name)
    {
        HashSet<byte> winners = Game.MatchData.GameHistory.LastWinners.Select(p => p.MyPlayer.PlayerId).ToHashSet();
        PlayerHistory? foundPlayer = PlayerHistories!.FirstOrDefault(p => p.Name == name);
        if (foundPlayer == null)
        {
            ErrorHandler(source).Message(PlayerNotFoundText.Formatted(name)).Send();
            return;
        }


        string coloredName = ((Color)Palette.PlayerColors[foundPlayer.Outfit.ColorId]).Colorize(ModConstants.ColorNames[foundPlayer.Outfit.ColorId]);

        string statusText = foundPlayer.Status is PlayerStatus.Dead ? foundPlayer.CauseOfDeath?.SimpleName() ?? foundPlayer.Status.ToString() : foundPlayer.Status.ToString();
        string playerStatus = StatusColor(foundPlayer.Status).Colorize(statusText);

        string winnerText = winners.Contains(foundPlayer.PlayerId) ? WinnerColor.Colorize(" ★ " + LRTranslations.WinnerText + " ★") : "";
        string titleText = $"{name} {coloredName}{winnerText}\n";
        titleText += $"{LRTranslations.RoleText.Formatted(foundPlayer.Role.RoleColor.Colorize(foundPlayer.Role.RoleName))} ({playerStatus})\n";
        if (foundPlayer.Subroles.Count > 0) titleText += LRTranslations.ModifierText.Formatted(foundPlayer.Subroles.Select(sr => sr.RoleColor.Colorize(sr.RoleName)).Fuse());

        string text = "";
        text += "\n\n";

        text += foundPlayer.Role.Statistics().Select(s => $"{s.Name()}: {s.GetGenericValue(foundPlayer.UniquePlayerId)}").Fuse("\n") + "\n.";

        ChatHandler.Of(text, titleText).LeftAlign().Send(source);
    }

    public static void GeneralResults(PlayerControl source)
    {
        if (PlayerHistories == null) return;

        HashSet<byte> winners = Game.MatchData.GameHistory.LastWinners.Select(p => p.MyPlayer.PlayerId).ToHashSet();
        string text = GeneralColor3.Colorize("<b>Translations.ResultsText</b>") + "\n";

        text = PlayerHistories.OrderBy(StatusOrder).Select(p => CreateSmallPlayerResult(p, winners.Contains(p.PlayerId)))
            .Fuse("<line-height=3.1>\n</line-height>");

        string winResult = new Optional<IWinCondition>(Game.GetWinDelegate().WinCondition()).Map(wc =>
        {
            string t;
            if (wc is IFactionWinCondition factionWin) t = factionWin.Factions().Select(f => f.FactionColor().Colorize(f.Name())).Fuse();
            else t = Game.MatchData.GameHistory.LastWinners.Select(lw => lw.Name).Fuse();
            return $"\n\n<size=1.8>{LRTranslations.WinResultText.Formatted(t)}</size>";
        }).OrElse("");


        string content = text + winResult;
        DevLogger.Log(content);

        ChatHandler.Of("\n".Repeat(PlayerHistories.Count - 1), content).LeftAlign().Send(source);
        return;

        float DeathTimeOrder(PlayerHistory ph) => ph.CauseOfDeath == null ? 0f : (7200f - (float)ph.CauseOfDeath.Timestamp().TimeSpan().TotalSeconds) / 7200f;
        float StatusOrder(PlayerHistory ph) => winners.Contains(ph.PlayerId) ? (float)ph.Status + DeathTimeOrder(ph) : (float)ph.Status + 99 + DeathTimeOrder(ph);
    }

    private static string CreateSmallPlayerResult(PlayerHistory history, bool isWinner)
    {
        const string indent = "<indent=5.5%>{0}</indent>";
        string winnerPrefix = isWinner ? WinnerColor.Colorize("★ ") + "{0}" : indent;

        string statusText = history.Status is PlayerStatus.Dead ? history.CauseOfDeath?.SimpleName() ?? history.Status.ToString() : history.Status.ToString();
        string playerStatus = StatusColor(history.Status).Colorize(statusText);

        string statText = history.Role.Statistics().FirstOrOptional().Map(t => $" <size=1.5>[{t.Name()}: {t.GetGenericValue(history.UniquePlayerId)}]</size>").OrElse("");

        int colorId = history.Outfit.ColorId;
        string coloredName = ((Color)Palette.PlayerColors[colorId]).Colorize(ModConstants.ColorNames[colorId]);
        string modifiers = history.Subroles.Count == 0 ? "" : $"\n<size=1.4>{history.Subroles.Select(sr => sr.RoleColor.Colorize(sr.RoleName)).Fuse()}</size>";

        string totalText = winnerPrefix.Formatted($"{history.Name} : {coloredName} <size=1.5>({playerStatus})</size>\n{indent.Formatted($"<size=1.9>{history.Role.RoleColor.Colorize(history.Role.RoleName)}</size> {statText}{modifiers}")}");
        return $"<line-height=2.2>{totalText}</line-height>";
    }

    private static ChatHandler ErrorHandler(PlayerControl source) => new ChatHandler().Title(t => t.Text(CommandError).Color(KillingColor).Build()).Player(source).LeftAlign();

    private static Color StatusColor(PlayerStatus status)
    {
        return status switch
        {
            PlayerStatus.Alive => Color.green,
            PlayerStatus.Exiled => new Color(0.73f, 0.54f, 0.45f),
            PlayerStatus.Disconnected => Color.grey,
            PlayerStatus.Dead => Color.red,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    private static List<PlayerHistory>? PlayerHistories => Game.MatchData.GameHistory.PlayerHistory;

    [Localized("LastResults")]
    public static class LRTranslations
    {
        [Localized(nameof(NoPreviousGameText))]
        public static string NoPreviousGameText = "No game played yet!";

        [Localized(nameof(RoleText))]
        public static string RoleText = "<b>Role:</b> {0}";

        [Localized(nameof(ModifierText))]
        public static string ModifierText = "<b>Mods:</b> {0}";

        [Localized(nameof(WinnerText))]
        public static string WinnerText = "Winner";

        [Localized(nameof(ResultsText))]
        public static string ResultsText = "Results";

        [Localized(nameof(WinResultText), ForceOverride = true)]
        public static string WinResultText = "Winners: {0}";
    }
}