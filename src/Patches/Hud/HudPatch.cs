using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Options;
using UnityEngine;

namespace Lotus.Patches.Hud;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
class HudManagerPatch
{
    public static bool ShowDebugText = false;
    public static int LastCallNotifyRolesPerSecond = 0;
    public static int NowCallNotifyRolesCount = 0;
    public static int LastSetNameDesyncCount = 0;
    public static int LastFPS = 0;
    public static int NowFrameCount = 0;
    public static float FrameRateTimer = 0.0f;
    public static TMPro.TextMeshPro LowerInfoText;

    public static void Postfix(HudManager __instance)
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null) return;

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if ((!AmongUsClient.Instance.IsGameStarted || AmongUsClient.Instance.NetworkMode is not NetworkModes.OnlineGame)
                && player.CanMove)
            {
                player.Collider.offset = new Vector2(0f, 127f);
            }
        }
        //壁抜け解除
        if (player.Collider.offset.y == 127f)
        {
            if (!Input.GetKey(KeyCode.LeftControl) || (AmongUsClient.Instance.IsGameStarted && AmongUsClient.Instance.NetworkMode is NetworkModes.OnlineGame))
            {
                player.Collider.offset = new Vector2(0f, -0.3636f);
            }
        }

        if (!AmongUsClient.Instance.IsGameStarted) __instance.ReportButton.Hide();
        else if (Game.State is GameState.InMeeting) __instance.ReportButton.Hide();
        else if (!PlayerControl.LocalPlayer.IsAlive()) __instance.ReportButton.Hide();
        else __instance.ReportButton.Show();


        __instance.GameSettings.text = OptionShower.GetOptionShower().GetPage();
        //ゲーム中でなければ以下は実行されない
        if (!AmongUsClient.Instance.IsGameStarted) return;

    }
}

