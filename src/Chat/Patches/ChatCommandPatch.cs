using HarmonyLib;
using UnityEngine;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.Chat.Patches;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
public class ChatUpdatePatch
{
    public static void Postfix(ChatController __instance)
    {
        __instance.TextArea.AllowPaste = true;
        __instance.chatBubPool.Prefab.Cast<ChatBubble>().TextArea.overrideColorTags = false;
        __instance.TimeSinceLastMessage = 3f;
    }

    [QuickPostfix(typeof(ChatController), nameof(ChatController.UpdateCharCount))]
    public static void UpdateCharCount(ChatController __instance)
    {
        int length = __instance.TextArea.text.Length;
        __instance.CharCount.text = $"{length}/{__instance.TextArea.characterLimit}";
        if (length < (AmongUsClient.Instance.AmHost ? 1750 : 250))
            __instance.CharCount.color = Color.black;
        else if (length < (AmongUsClient.Instance.AmHost ? 2000 : 300))
            __instance.CharCount.color = new Color(1f, 1f, 0.0f, 1f);
        else
            __instance.CharCount.color = Color.red;
    }
}