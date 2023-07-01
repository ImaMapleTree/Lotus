using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.Utilities;
using LotusTrigger.Options;
using UnityEngine;
using VentLib.Localization;
using VentLib.Logging;


namespace Lotus.Patches.Network;

[HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
class PingTrackerPatch
{
    public static float deltaTime;
    private static bool dipped;
    static void Postfix(PingTracker __instance)
    {
        __instance.text.alignment = TMPro.TextAlignmentOptions.TopRight;

        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = Mathf.Ceil(1.0f / deltaTime);
        if (fps < 30 && !dipped && Game.State is GameState.Roaming)
        {
            VentLogger.High($"FPS Dipped Below 30 => {fps}");
            dipped = true;
        }
        else dipped = false;

        __instance.text.text += " " + fps + " fps";
        __instance.text.sortingOrder = -1;


        __instance.text.text += ProjectLotus.CredentialsText;
        if (GeneralOptions.DebugOptions.NoGameEnd) __instance.text.text += $"\r\n" + Utils.ColorString(Color.red, Localizer.Translate("StaticOptions.NoGameEnd"));
        __instance.text.text += $"\r\n" + Game.CurrentGamemode.Name;



        var offsetX = 1.2f; //右端からのオフセット
        if (HudManager.InstanceExists && HudManager._instance.Chat.ChatButton.active) offsetX += 0.8f; //チャットボタンがある場合の追加オフセット
        if (FriendsListManager.InstanceExists && FriendsListManager._instance.FriendsListButton.Button.active) offsetX += 0.8f; //フレンドリストボタンがある場合の追加オフセット
        __instance.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(offsetX, 0f, 0f);
    }
}