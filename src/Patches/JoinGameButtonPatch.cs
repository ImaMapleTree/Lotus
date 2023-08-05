using System.Text.RegularExpressions;
using HarmonyLib;
using UnityEngine;

namespace Lotus.Patches;

[HarmonyPatch(typeof(JoinGameButton), nameof(JoinGameButton.OnClick))]
class JoinGameButtonPatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(JoinGameButtonPatch));

    public static void Prefix(JoinGameButton __instance)
    {
        if (__instance.GameIdText == null) return;
        if (__instance.GameIdText.text == "" && Regex.IsMatch(GUIUtility.systemCopyBuffer.Trim('\r', '\n'), @"^[A-Z]{6}$"))
        {
            log.Info($"{GUIUtility.systemCopyBuffer}", "ClipBoard");
            __instance.GameIdText.SetText(GUIUtility.systemCopyBuffer.Trim('\r', '\n'));
        }
    }
}