using System.Collections.Generic;
using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using InnerNet;
using Lotus.API;
using Lotus.Options.LotusImpl;
using LotusTrigger.Options;
using UnityEngine;
using VentLib.Utilities.Extensions;
using VentLib.Logging;
using VentLib.Utilities;

namespace Lotus.Patches;

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class GameStartManagerUpdatePatch
{
    public static void Prefix(GameStartManager __instance)
    {
        __instance.MinPlayers = 1;
    }
}
//タイマーとコード隠し
public static class GameStartManagerPatch
{
    private static float timer = 600f;
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    public class GameStartManagerStartPatch
    {
        public static TMPro.TextMeshPro HideName;
        public static void Postfix(GameStartManager __instance)
        {
            __instance.GameRoomNameCode.text = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
            // Reset lobby countdown timer
            timer = 600f;

            HideName = Object.Instantiate(__instance.GameRoomNameCode, __instance.GameRoomNameCode.transform);
            /*HideName.text = ColorUtility.TryParseHtmlString(TOHPlugin.HideColor.Value, out _)
                ? $"<color={TOHPlugin.HideColor.Value}>{TOHPlugin.HideName.Value}</color>"
                : $"<color={TOHPlugin.ModColor}>{TOHPlugin.HideName.Value}</color>";*/

            // Make Public Button
            /*if ((ModUpdater.isBroken || ModUpdater.hasUpdate || !TOHPlugin.AllowPublicRoom) && !ModUpdater.ForceAccept)
            {
                __instance.MakePublicButton.color = Palette.DisabledClear;
                __instance.privatePublicText.color = Palette.DisabledClear;
            }*/
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public class GameStartManagerUpdatePatch
    {
        private static bool update = false;
        private static string currentText = "";
        public static void Prefix(GameStartManager __instance)
        {
            // Lobby code
            if (DataManager.Settings.Gameplay.StreamerMode)
            {
                __instance.GameRoomNameCode.color = new(255, 255, 255, 0);
                GameStartManagerStartPatch.HideName.enabled = true;
            }
            else
            {
                __instance.GameRoomNameCode.color = new(255, 255, 255, 255);
                GameStartManagerStartPatch.HideName.enabled = false;
            }
            if (!AmongUsClient.Instance.AmHost || !GameData.Instance || AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame) return; // Not host or no instance or LocalGame
            update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
        }
        public static void Postfix(GameStartManager __instance)
        {
            // Lobby timer
            if (!AmongUsClient.Instance.AmHost || !GameData.Instance || AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame) return;

            if (update) currentText = __instance.PlayerCounter.text;

            timer = Mathf.Max(0f, timer -= Time.deltaTime);
            int minutes = (int)timer / 60;
            int seconds = (int)timer % 60;
            string suffix = $" ({minutes:00}:{seconds:00})";
            if (timer <= 60) suffix = Color.red.Colorize(suffix);

            __instance.PlayerCounter.text = currentText + suffix;
            __instance.PlayerCounter.autoSizeTextContainer = true;
        }
    }
    [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
    public static class HiddenTextPatch
    {
        private static void Postfix(TextBoxTMP __instance)
        {
            if (__instance.name == "GameIdText") __instance.outputText.text = new string('*', __instance.text.Length);
        }
    }
}
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
public class GameStartRandomMap
{
    public static bool Prefix(GameStartManager __instance)
    {
        if (GeneralOptions.AdminOptions.HostGM) VentLogger.SendInGame("[Info] GM is Enabled");

        __instance.ReallyBegin(false);
        return false;
    }
    public static bool Prefix(GameStartRandomMap __instance)
    {
        AUSettings.StaticOptions.SetFloat(FloatOptionNames.ProtectionDurationSeconds, 3600f);
        AUSettings.StaticOptions.SetBool(BoolOptionNames.ImpostorsCanSeeProtect, false);
        if (!GeneralOptions.MayhemOptions.UseRandomMap) return true;

        List<byte> randomMaps = new();

        AuMap map = GeneralOptions.MayhemOptions.RandomMaps;
        if (map.HasFlag(AuMap.Skeld)) randomMaps.Add(0);
        if (map.HasFlag(AuMap.Mira)) randomMaps.Add(1);
        if (map.HasFlag(AuMap.Polus)) randomMaps.Add(2);
        if (map.HasFlag(AuMap.Airship)) randomMaps.Add(4);



        if (randomMaps.Count == 0) return true;

        AUSettings.StaticOptions.SetByte(ByteOptionNames.MapId, randomMaps.GetRandom());
        return true;
    }
}

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.ResetStartState))]
class ResetStartStatePatch
{
    public static void Prefix()
    {
        if (GameStartManager.InstanceExists && GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown)
        {
            PlayerControl.LocalPlayer.RpcSyncSettings(GameOptionsManager.Instance.gameOptionsFactory.ToBytes(GameOptionsManager.Instance.CurrentGameOptions));
        }
    }
}
[HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
class UnrestrictedNumImpostorsPatch
{
    public static bool Prefix(ref int __result)
    {
        __result = ProjectLotus.NormalOptions.NumImpostors;
        return false;
    }
}