using System.Collections.Generic;
using HarmonyLib;
using VentLib.Logging;

namespace TOHTOR.Chat.Patches;

[HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetRight))]
class ChatBubblePatch
{
    internal static Queue<int> SetRightQueue = new();

    public static void Postfix(ChatBubble __instance)
    {
        if (SetRightQueue.TryDequeue(out int _))
        {
            VentLogger.Fatal($"Setting left: {__instance.TextArea.text}");
            __instance.SetLeft();
        }
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