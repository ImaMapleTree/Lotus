using HarmonyLib;
using Lotus.GUI.Menus.OptionsMenu;
using Lotus.Options;
using TMPro;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Patches.Client;

[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.ShowButtons))]
public class EndGameManagerPatch
{
    public static bool IsRestarting { get; private set; }
    [Localized($"{ModConstants.Localization.Misc}.PlayAgainText")]
    private static string _playAgainText = "Restarting In: {0}";

    private static TextMeshPro autoPlayAgainText;

    public static void Postfix(EndGameManager __instance)
    {
        if (!AmongUsClient.Instance.AmHost || !GeneralOptions.AdminOptions.AutoPlayAgain) return;
        IsRestarting = false;

        Async.Schedule(() =>
        {
            VentLogger.High("Beginning Auto Play Again Countdown!", "AutoPlayAgain");
            IsRestarting = true;
            BeginAutoPlayAgainCountdown(__instance, 5);
        }, 0.5f);
    }

    public static void CancelPlayAgain()
    {
        IsRestarting = false;
        if (autoPlayAgainText != null) autoPlayAgainText.gameObject.SetActive(false);
    }

    private static void BeginAutoPlayAgainCountdown(EndGameManager endGameManager, int seconds)
    {
        if (!IsRestarting) return;
        if (endGameManager == null) return;
        EndGameNavigation navigation = endGameManager.Navigation;
        if (navigation == null) return;

        if (autoPlayAgainText == null)
        {
            autoPlayAgainText = Object.Instantiate(navigation.gameObject.GetComponentInChildren<TextMeshPro>(), navigation.transform);
            autoPlayAgainText.font = CustomOptionContainer.GetGeneralFont();
            autoPlayAgainText.fontSize += 6;
            autoPlayAgainText.color = Color.white;
            autoPlayAgainText.transform.localScale += new Vector3(0.25f, 0.25f);
            Async.Schedule(() => autoPlayAgainText.text = _playAgainText.Formatted(seconds.ToString()), 0.001f);
            autoPlayAgainText.transform.localPosition += new Vector3(3.5f, 2.6f);
        }

        autoPlayAgainText.text = _playAgainText.Formatted(seconds.ToString());
        if (seconds == 0) navigation.NextGame();
        else Async.Schedule(() => BeginAutoPlayAgainCountdown(endGameManager, seconds - 1), 1.1f);
    }
}