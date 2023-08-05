using HarmonyLib;

namespace Lotus.GUI.Patches;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.Toggle))]
class CancelBanMenuStuckPatch
{
    public static void Prefix(ChatController __instance)
    {
        if (__instance.state is ChatControllerState.Open && !__instance.IsAnimating) // (IsOpen==true) == 今から閉じないといけない
        {
            // BanButtonを非表示にする
            __instance.banButton.SetVisible(false);
        }
    }
}