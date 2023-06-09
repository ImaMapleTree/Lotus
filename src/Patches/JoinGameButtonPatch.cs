using System.Text.RegularExpressions;
using HarmonyLib;
using UnityEngine;
using VentLib.Logging;

namespace Lotus.Patches
{
    [HarmonyPatch(typeof(JoinGameButton), nameof(JoinGameButton.OnClick))]
    class JoinGameButtonPatch
    {
        public static void Prefix(JoinGameButton __instance)
        {
            if (__instance.GameIdText == null) return;
            if (__instance.GameIdText.text == "" && Regex.IsMatch(GUIUtility.systemCopyBuffer.Trim('\r', '\n'), @"^[A-Z]{6}$"))
            {
                VentLogger.Old($"{GUIUtility.systemCopyBuffer}", "ClipBoard");
                __instance.GameIdText.SetText(GUIUtility.systemCopyBuffer.Trim('\r', '\n'));
            }
        }
    }
}