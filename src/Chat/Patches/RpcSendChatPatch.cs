using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using Lotus.Managers.Hotkeys;
using UnityEngine;
using VentLib.Networking.RPC;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.Chat.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
internal class RpcSendChatPatch
{
    private static readonly List<string> ChatHistory = new();
    private static int _index = -1;

    static RpcSendChatPatch()
    {
        HotkeyManager.Bind(KeyCode.UpArrow)
            .If(b => b.Predicate(() =>
            {
                if (_index + 1 >= ChatHistory.Count) return false;
                if (HudManager.Instance == null || HudManager.Instance.Chat == null) return false;
                return HudManager.Instance.Chat.freeChatField.textArea.hasFocus;
            })).Do(BackInChatHistory);
        HotkeyManager.Bind(KeyCode.DownArrow)
            .If(b => b.Predicate(() =>
            {
                if (ChatHistory.Count == 0) return false;
                if (HudManager.Instance == null || HudManager.Instance.Chat == null) return false;
                return HudManager.Instance.Chat.freeChatField.textArea.hasFocus;
            })).Do(ForwardInChatHistory);
    }

    private static void BackInChatHistory() => HudManager.Instance.Chat.freeChatField.textArea.SetText(_index + 1 >= ChatHistory.Count ? ChatHistory[_index] : ChatHistory[++_index]);

    private static void ForwardInChatHistory()
    {
        string text = "";
        if (_index == -1) text = "";
        else if (_index == 0)
        {
            text = "";
            _index = -1;
        }
        else text = ChatHistory[--_index];

        HudManager.Instance.Chat.freeChatField.textArea.SetText(text);
    }

    internal static bool EatCommand;
    public static bool Prefix(PlayerControl __instance, string chatText)
    {
        _index = -1;
        if (string.IsNullOrWhiteSpace(chatText))
            return false;

        if (!EatCommand) RpcV3.Standard(__instance.NetId, RpcCalls.SendChat, SendOption.None).Write(chatText).Send();

        OnChatPatch.EatMessage = EatCommand;
        if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
            DestroyableSingleton<HudManager>.Instance.Chat.AddChat(__instance, chatText);

        EatCommand = false;

        if (ChatHistory.Count == 0 || ChatHistory[0] != chatText)
            ChatHistory.Insert(0, chatText);
        if (ChatHistory.Count >= 100) ChatHistory.RemoveAt(99);



        return false;
    }

    [QuickPostfix(typeof(TextBoxTMP), nameof(TextBoxTMP.LoseFocus))]
    private static void LoseFocus()
    {
        _index = -1;
    }
}