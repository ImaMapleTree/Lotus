using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using TOHTOR.API;
using TOHTOR.API.Reactive;
using TOHTOR.API.Reactive.HookEvents;
using TOHTOR.Extensions;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Utilities;
using VentLib.Utilities;

namespace TOHTOR.Chat.Patches;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
internal static class OnChatPatch
{
    internal static List<byte> UtilsSentList = new();

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal static void Prefix(ChatController __instance, PlayerControl sourcePlayer, string chatText)
    {
        if (UtilsSentList.Count > 0 && UtilsSentList.Contains(sourcePlayer.PlayerId))
        {
            UtilsSentList.RemoveAt(UtilsSentList.FindIndex(b => b == sourcePlayer.PlayerId));
            return;
        }
        Hooks.PlayerHooks.PlayerMessageHook.Propagate(new PlayerMessageHookEvent(sourcePlayer, chatText));
        if (!TOHPlugin.PluginDataManager.ChatManager.HasBannedWord(chatText) || sourcePlayer.IsHost())
        {
            if (Game.State is GameState.InLobby) return;
            ActionHandle handle = ActionHandle.NoInit();
            Game.TriggerForAll(RoleActionType.Chat, ref handle, sourcePlayer, chatText, Game.State, sourcePlayer.IsAlive());
            return;
        }
        AmongUsClient.Instance.KickPlayer(sourcePlayer.GetClientId(), false);
        Utils.SendMessage($"{sourcePlayer.UnalteredName()} was kicked by AutoKick.");
    }
}