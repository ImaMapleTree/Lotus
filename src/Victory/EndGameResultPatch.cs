/*using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Logging;
using Lotus.Managers.History;
using Lotus.Roles;
using Lotus.Roles.Extra;
using TMPro;
using UnityEngine;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using Object = UnityEngine.Object;

namespace Lotus.Victory;

[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
public class EndGameResultPatch
{
    public static void Postfix(EndGameManager __instance)
    {
        __instance.WinText.alignment = TextAlignmentOptions.Bottom;

        TextMeshPro resultsText = Object.Instantiate(__instance.WinText);
        Transform resultTextTransform = resultsText.transform;

        var cameraPosition = Camera.main!.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
        resultTextTransform.position = new Vector3(__instance.Navigation.ExitButton.transform.position.x + 0.1f, cameraPosition.y - 0.1f, -15f);
        resultTextTransform.localScale = new Vector3(1f, 1f, 1f);

        resultsText.alignment = TextAlignmentOptions.TopLeft;
        resultsText.color = Color.white;
        resultsText.outlineWidth *= 1.2f;
        resultsText.fontSizeMin = resultsText.fontSizeMax = resultsText.fontSize = 1.25f;
        resultsText.fontSizeMin = 3f;
        resultsText.text = "";

        HashSet<byte> winners = Game.MatchData.GameHistory.LastWinners.Select(p => p.MyPlayer.PlayerId).ToHashSet();

        TextTable textTable = new("Name", "Stats", "Status", "Roles");

        PlayerHistories!
            .Where(ph => ph.Role is not GM)
            .OrderBy(StatusOrder)
            .ForEach(p => CreateSmallPlayerResult(p, winners.Contains(p.PlayerId), textTable));

        string table = textTable.ToString();
        resultsText.text = Color.white.Colorize(table);

        float StatusOrder(PlayerHistory ph) => winners.Contains(ph.PlayerId) ? (float)ph.Status + DeathTimeOrder(ph) : (float)ph.Status + 99 + DeathTimeOrder(ph);
    }

    private static void CreateSmallPlayerResult(PlayerHistory history, bool isWinner, TextTable textTable)
    {
        string winnerPrefix = isWinner ? ModConstants.Palette.WinnerColor.Colorize("★ ") : "　 ";

        string statusText = history.Status is PlayerStatus.Dead ? history.CauseOfDeath?.SimpleName() ?? history.Status.ToString() : history.Status.ToString();
        string playerStatus = StatusColor(history.Status).Colorize(statusText);

        string statText = history.Role.Statistics().FirstOrOptional().Map(t => $"{t.Name()}: {t.GetGenericValue(history.UniquePlayerId)}").OrElse("");

        int colorId = history.Outfit.ColorId;
        string coloredName = ((Color)Palette.PlayerColors[colorId]).Colorize(history.Name);
        string roles = new List<CustomRole> { history.Role }.Concat(history.Subroles).Select(r => r.ColoredRoleName()).Fuse(" + ");

        textTable.AddEntry(winnerPrefix + coloredName, statText, playerStatus, roles);
    }

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

    static float DeathTimeOrder(PlayerHistory ph) => ph.CauseOfDeath == null ? 0f : (7200f - (float)ph.CauseOfDeath.Timestamp().TimeSpan().TotalSeconds) / 7200f;
    private static List<PlayerHistory>? PlayerHistories => Game.MatchData.GameHistory.PlayerHistory;
}*/