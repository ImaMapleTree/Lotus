using HarmonyLib;
using UnityEngine;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.Chat.Patches;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
public class ChatUpdatePatch
{
    public static void Postfix(ChatController __instance)
    {
        __instance.freeChatField.textArea.AllowPaste = true;
        __instance.chatBubblePool.Prefab.Cast<ChatBubble>().TextArea.overrideColorTags = false;
        __instance.timeSinceLastMessage = 3f;
    }

    [QuickPostfix(typeof(FreeChatInputField), nameof(FreeChatInputField.UpdateCharCount))]
    public static void UpdateCharCount(FreeChatInputField __instance)
    {
        int length = __instance.textArea.text.Length;
        __instance.charCountText.text = $"{length}/{__instance.textArea.characterLimit}";
        if (length < (AmongUsClient.Instance.AmHost ? 1750 : 250))
            __instance.charCountText.color = Color.black;
        else if (length < (AmongUsClient.Instance.AmHost ? 2000 : 300))
            __instance.charCountText.color = new Color(1f, 1f, 0.0f, 1f);
        else
            __instance.charCountText.color = Color.red;
    }
}