using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using TOHTOR.API;
using TOHTOR.API.Reactive;
using TOHTOR.API.Reactive.HookEvents;
using TOHTOR.Extensions;
using TOHTOR.Managers;
using TOHTOR.Options;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Utilities;
using VentLib.Logging;
using VentLib.Utilities;

namespace TOHTOR.Chat.Patches;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
internal static class OnChatPatch
{
    internal static List<byte> UtilsSentList = new();

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal static void Prefix(ChatController __instance, PlayerControl sourcePlayer, string chatText)
    {
        VentLogger.Log(LogLevel.All, $"{sourcePlayer.UnalteredName()} => {chatText}");
        if (UtilsSentList.Contains(sourcePlayer.PlayerId))
        {
            VentLogger.Trace($"Filtered Util Message Sent By: {sourcePlayer.UnalteredName()}");
            UtilsSentList.RemoveAt(UtilsSentList.FindIndex(b => b == sourcePlayer.PlayerId));
            return;
        }
        Hooks.PlayerHooks.PlayerMessageHook.Propagate(new PlayerMessageHookEvent(sourcePlayer, chatText));
        if (!UseWordList() || !PluginDataManager.ChatManager.HasBannedWord(chatText) || sourcePlayer.IsHost())
        {
            if (Game.State is GameState.InLobby) return;
            ActionHandle handle = ActionHandle.NoInit();
            Game.TriggerForAll(RoleActionType.Chat, ref handle, sourcePlayer, chatText, Game.State, sourcePlayer.IsAlive());
            return;
        }
        AmongUsClient.Instance.KickPlayer(sourcePlayer.GetClientId(), false);
        Utils.SendMessage($"{sourcePlayer.UnalteredName()} was kicked by AutoKick.");
    }

    public static bool UseWordList() => GeneralOptions.AdminOptions.AutoKick;
}