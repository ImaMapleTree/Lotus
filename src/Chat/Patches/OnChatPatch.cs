using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Managers;
using Lotus.Options;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Utilities;
using Lotus.API;
using Lotus.Extensions;
using Lotus.Logging;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Chat.Patches;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
internal static class OnChatPatch
{
    internal static List<byte> UtilsSentList = new();
    internal static bool EatMessage;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal static bool Prefix(ChatController __instance, PlayerControl sourcePlayer, string chatText)
    {
        VentLogger.Log(LogLevel.All, $"{sourcePlayer.name} => {chatText}");
        if (UtilsSentList.Contains(sourcePlayer.PlayerId))
        {
            UtilsSentList.RemoveAt(UtilsSentList.FindIndex(b => b == sourcePlayer.PlayerId));
            return true;
        }
        Hooks.PlayerHooks.PlayerMessageHook.Propagate(new PlayerMessageHookEvent(sourcePlayer, chatText));
        if (!UseWordList() || !PluginDataManager.ChatManager.HasBannedWord(chatText) || sourcePlayer.IsHost())
        {
            if (PluginDataManager.TemplateCommandManager.CheckAndRunCommand(sourcePlayer, chatText)) return true;
            bool eat = EatMessage;
            EatMessage = false;
            if (Game.State is GameState.InLobby) return !eat;
            ActionHandle handle = ActionHandle.NoInit();
            Game.TriggerForAll(RoleActionType.Chat, ref handle, sourcePlayer, chatText, Game.State, sourcePlayer.IsAlive());
            return !eat;
        }
        AmongUsClient.Instance.KickPlayer(sourcePlayer.GetClientId(), false);
        ChatHandler.Send($"{sourcePlayer.name} was kicked by AutoKick.");
        return true;
    }

    public static bool UseWordList() => GeneralOptions.AdminOptions.AutoKick;
}