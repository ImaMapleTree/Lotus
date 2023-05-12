using System.Collections.Generic;
using HarmonyLib;
using VentLib.Logging;
using VentLib.Utilities.Harmony.Attributes;

namespace TOHTOR.Chat.Patches;


class ChatBubblePatch
{
    internal static Queue<int> SetLeftQueue = new();

    [QuickPostfix(typeof(ChatBubble), nameof(ChatBubble.SetRight))]
    public static void SetBubbleRight(ChatBubble __instance)
    {
        if (SetLeftQueue.TryDequeue(out int _)) __instance.SetLeft();

        __instance.TextArea.richText = true;
    }

    [QuickPostfix(typeof(ChatBubble), nameof(ChatBubble.SetLeft))]
    public static void SetBubbleLeft(ChatBubble __instance)
    {
        __instance.TextArea.richText = true;
    }
}


/*[HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
class ChatBubbleSetNamePatch
{
    public static void Prefix(ChatBubble __instance)
    {
        PlayerControl relatedPlayer = __instance.playerInfo.Object;
        if (relatedPlayer == null) return;
        DynamicName name = relatedPlayer.GetDynamicName();
        relatedPlayer.RpcSetName(name.RawName);
        __instance.NameText.color = Color.white;
    }

    /*public static void Postfix(ChatBubble __instance)
    {
        PlayerControl relatedPlayer = __instance.playerInfo.Object;
        if (relatedPlayer == null) return; ;
    }#1#
}*/