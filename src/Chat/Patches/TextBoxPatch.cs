using HarmonyLib;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.Chat.Patches;

[HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.IsCharAllowed))]
public class TextBoxPatch
{
    public static void Postfix(TextBoxTMP __instance, char i, ref bool __result)
    {
        if (i is '@' or '$' or '_') __result = true;
    }

    [QuickPrefix(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
    public static void ModifyCharacterLimit(TextBoxTMP __instance) => __instance.characterLimit = AmongUsClient.Instance.AmHost ? 2000 : 300;
}