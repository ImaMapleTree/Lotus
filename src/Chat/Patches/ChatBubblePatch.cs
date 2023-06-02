using System.Collections.Generic;
using HarmonyLib;
using VentLib.Logging;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.Chat.Patches;

static class ChatBubblePatch
{
    internal static readonly Queue<int> SetLeftQueue = new();

    [QuickPostfix(typeof(ChatBubble), nameof(ChatBubble.SetRight))]
    public static void SetBubbleRight(ChatBubble __instance)
    {
        if (SetLeftQueue.TryDequeue(out int _)) __instance.SetLeft();

        __instance.TextArea.richText = true;
    }

    [QuickPostfix(typeof(ChatBubble), nameof(ChatBubble.SetLeft))]
    public static void SetBubbleLeft(ChatBubble __instance)
    {
        SetLeftQueue.TryDequeue(out int _);
        __instance.TextArea.richText = true;
    }
}