using HarmonyLib;
using Lotus.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.Chat.Patches;

[HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.IsCharAllowed))]
public class TextBoxPatch
{
    public static void Postfix(TextBoxTMP __instance, char i, ref bool __result)
    {
        /*Async.Schedule(() => DevLogger.Log($"{__instance.text} | {__instance.text.Length} | {(int)__instance.text[0]}"), 0.01f);*/
        __result = i != 8 && i is not ('\r' or '\n');
    }

    [QuickPrefix(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
    public static void ModifyCharacterLimit(TextBoxTMP __instance) => __instance.characterLimit = AmongUsClient.Instance.AmHost ? 2000 : 300;
}