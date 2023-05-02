using HarmonyLib;
using Hazel;
using VentLib.Networking.RPC;

namespace TOHTOR.Chat.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
internal class RpcSendChatPatch
{
    public static bool Prefix(PlayerControl __instance, string chatText)
    {
        if (string.IsNullOrWhiteSpace(chatText))
            return false;

        RpcV3.Standard(__instance.NetId, RpcCalls.SendChat, SendOption.None).Write(chatText).Send();

        if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
            DestroyableSingleton<HudManager>.Instance.Chat.AddChat(__instance, chatText);

        return false;
    }
}