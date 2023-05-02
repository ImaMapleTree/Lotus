using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using TOHTOR.API;
using TOHTOR.Options;
using UnityEngine;

namespace TOHTOR.GUI.Hud;

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
        var taskTextPrefix = "";
        DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FakeTasks, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
        //壁抜け
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if ((!AmongUsClient.Instance.IsGameStarted || !GameStates.IsOnlineGame)
                && player.CanMove)
            {
                player.Collider.offset = new Vector2(0f, 127f);
            }
        }
        //壁抜け解除
        if (player.Collider.offset.y == 127f)
        {
            if (!Input.GetKey(KeyCode.LeftControl) || (AmongUsClient.Instance.IsGameStarted && GameStates.IsOnlineGame))
            {
                player.Collider.offset = new Vector2(0f, -0.3636f);
            }
        }


        __instance.GameSettings.text = OptionShower.GetOptionShower().GetPage();
        //ゲーム中でなければ以下は実行されない
        if (!AmongUsClient.Instance.IsGameStarted) return;

        if (!Input.GetKeyDown(KeyCode.Y) || AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay) return;

        __instance.ToggleMapVisible(new MapOptions()
        {
            Mode = MapOptions.Modes.Sabotage,
            AllowMovementWhileMapOpen = true
        });

        if (!player.AmOwner) return;

        player.MyPhysics.inputHandler.enabled = true;
        ConsoleJoystick.SetMode_Task();
    }
}

