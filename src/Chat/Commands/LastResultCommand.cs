using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.Extensions;
using Lotus.Managers.History;
using Lotus.Options;
using UnityEngine;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;
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
            if (AmongUsClient.Instance.NetworkMode is not NetworkModes.LocalGame && PlayerHistories.All(p => p.UniquePlayerId != versionEvent.Player.UniquePlayerId())) return;
            GeneralResults(versionEvent.Player);
        });
        Hooks.NetworkHooks.GameJoinHook.Bind(nameof(LastResultCommand), gameJoinEvent =>
        {
            if (!GeneralOptions.MiscellaneousOptions.AutoDisplayResults) return;
            if (gameJoinEvent.IsNewLobby) return;
            Async.WaitUntil(() => PlayerControl.LocalPlayer, p => p != null, GeneralResults, 0.1f, 30);
        });
    }
    
    [Command("last", "l")]
    public static void LastGame(PlayerControl source, CommandContext context)
    {
        if (PlayerHistories == null) ErrorHandler(source).Message(Translations.NoPreviousGameText).Send();
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
        

        Color playerColor = Palette.PlayerColors[foundPlayer.Outfit.ColorId];

        string statusText = foundPlayer.Status is PlayerStatus.Dead ? foundPlayer.CauseOfDeath ?? foundPlayer.Status.ToString() : foundPlayer.Status.ToString();
        string playerStatus = StatusColor(foundPlayer.Status).Colorize(statusText);

        string winnerText = winners.Contains(foundPlayer.PlayerId) ? WinnerColor.Colorize("★ " + Translations.WinnerText + " ★") : "";
        string titleText = $"{playerColor.Colorize($"{name} {winnerText}")}\n";
        titleText += $"{Translations.RoleText.Formatted(foundPlayer.Role.RoleColor.Colorize(foundPlayer.Role.RoleName))} ({playerStatus})\n";
        if (foundPlayer.Subroles.Count > 0) titleText += Translations.ModifierText.Formatted(foundPlayer.Subroles.Select(sr => sr.RoleColor.Colorize(sr.RoleName)).Fuse());

        string text = "";
        text += "\n\n";
        
        text += foundPlayer.Role.Statistics().Select(s => $"{s.Name()}: {s.GetGenericValue(foundPlayer.UniquePlayerId)}").Fuse("\n") + "\n.";

        ChatHandler.Of(text, titleText).LeftAlign().Send(source);
    }
    
    public static void GeneralResults(PlayerControl source)
    {
        HashSet<byte> winners = Game.MatchData.GameHistory.LastWinners.Select(p => p.MyPlayer.PlayerId).ToHashSet();
        string text = GeneralColor3.Colorize("<b>Translations.ResultsText</b>") + "\n";
        text = PlayerHistories!.OrderBy(p => winners.Contains(p.PlayerId) ? (int)p.Status : 99 + (int)p.Status).Select(p => CreateSmallPlayerResult(p, winners.Contains(p.PlayerId)))
            .Fuse("<line-height=3.1>\n</line-height>");
        ChatHandler.Of("\n".Repeat(PlayerHistories!.Count - 1) + ".", text).LeftAlign().Send(source);
    }

    private static string CreateSmallPlayerResult(PlayerHistory history, bool isWinner)
    {
        string indent = "    ";
        string winnerPrefix = isWinner ? WinnerColor.Colorize("★ ") : indent;
        
        string statusText = history.Status is PlayerStatus.Dead ? history.CauseOfDeath ?? history.Status.ToString() : history.Status.ToString();
        string playerStatus = StatusColor(history.Status).Colorize(statusText);
        
        string statText = history.Role.Statistics().FirstOrOptional().Map(t => $" <size=1.5>[{t.Name()}: {t.GetGenericValue(history.UniquePlayerId)}]</size>").OrElse("");
        
        return $"<line-height=1.73>{winnerPrefix}{history.Name} <size=1.6>({playerStatus})</size>\n{indent}<size=1.9>{history.Role.RoleColor.Colorize(history.Role.RoleName)}</size> {statText}</line-height>";
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
    private static class Translations
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
    }
}