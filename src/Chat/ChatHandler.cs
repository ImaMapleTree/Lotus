using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Hazel;
using Lotus.API.Odyssey;
using Lotus.Chat.Patches;
using Lotus.Logging;
using Lotus.Managers;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Networking;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Chat;

[Localized("Announcements")]
[SuppressMessage("ReSharper", "ParameterHidesMember")]
public class ChatHandler
{
    private static int _maxMessagePacketSize = NetworkRules.MaxPacketSize + 200;

    [Localized("SystemMessage")]
    private static string _defaultTitle = "System Announcement";
    private static Color _defaultColor = new(0.67f, 0.67f, 1f);

    private PlayerControl? target;
    private string title = _defaultColor.Colorize(_defaultTitle);
    private string? message;

    private bool leftAligned;

    public ChatHandler Title(string title)
    {
        this.title = title;
        return this;
    }

    public ChatHandler Title(Func<TitleBuilder, string> titleBuilder)
    {
        title = titleBuilder(new TitleBuilder());
        return this;
    }

    public ChatHandler Message(string message, params object[] args)
    {
        if (args.Length > 0) message = message.Formatted(args);
        this.message = message.Replace("\\n", "\n");
        return this;
    }

    public ChatHandler Message(string message, bool leftAligned)
    {
        this.message = message;
        this.leftAligned = leftAligned;
        return this;
    }

    public ChatHandler Player(PlayerControl player)
    {
        this.target = player;
        return this;
    }

    public ChatHandler LeftAlign() => LeftAlign(true);

    public ChatHandler LeftAlign(bool leftAligned)
    {
        this.leftAligned = leftAligned;
        return this;
    }

    public void Send() => Send((PlayerControl)null!);

    public void Send(PlayerControl? targetPlayer)
    {
        if (targetPlayer == null) targetPlayer = target;
        Send(targetPlayer, message ?? "", title, leftAligned);
    }

    public void Send(UnityOptional<PlayerControl> targetPlayer) => targetPlayer.IfPresent(Send);

    public static ChatHandler Of(string? message = null, string? title = null)
    {
        ChatHandler ch = new();
        ch.message = message?.Replace("\\n", "\n");
        if (title != null) ch.title = title;
        return ch;
    }

    public static void Send(string message) => Send(null!, message, null);

    public static void Send(string message, string title) => Send(null, message, title);

    public static void Send(PlayerControl? player, string message, string? title = null, bool leftAligned = false)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        PlayerControl? sender;
        Async.Schedule(() =>
        {
            sender = Game.GetAlivePlayers().FirstOrDefault();
            if (sender == null) return;

            string name = sender.name;

            title ??= _defaultTitle;

            DevLogger.Log($"Player: {player} || NUll: {player == null}");
            if (player == null) MassSend(sender, message, title, leftAligned);
            else if (player.IsHost()) SendToHost(sender, message, title, leftAligned);
            else if (title.Length < _maxMessagePacketSize) InternalSendLM(sender, player, message, title, name);
            else InternalSendLT(sender, player, message, title, name);

            if (PluginDataManager.TitleManager.HasTitle(sender)) PluginDataManager.TitleManager.ApplyTitleWithChatFix(sender);
        }, 0.125f);
    }

    private static void SendToHost(PlayerControl sender, string message, string title, bool leftAligned)
    {
        message = message.Replace("@n", "\n");
        title = title.Replace("@n", "\n");
        if (leftAligned) ChatBubblePatch.SetLeftQueue.Enqueue(0);
        string name = sender.name;
        sender.SetName(title);
        OnChatPatch.UtilsSentList.Add(sender.PlayerId);
        DestroyableSingleton<HudManager>.Instance.Chat.AddChat(sender, message);
        sender.SetName(name);
    }

    private static void MassSend(PlayerControl sender, string message, string title, bool leftAligned)
    {
        string name = sender.name;
        PlayerControl.AllPlayerControls.ToArray().ForEach(p =>
        {
            if (p.IsHost()) SendToHost(sender, message, title, leftAligned);
            else if (title.Length < _maxMessagePacketSize) InternalSendLM(sender, p, message, title, name);
            else InternalSendLT(sender, p, message, title, name);
        });
    }

    private static void InternalSendLM(PlayerControl sender, PlayerControl recipient, string message, string title, string originalName)
    {
        int leftIndex = 0;
        int rightIndex = Math.Min(message.Length, _maxMessagePacketSize);

        while (rightIndex < message.Length)
        {
            string subMessage = message[leftIndex..rightIndex];
            leftIndex = FindGoodSplitPoint(ref subMessage, leftIndex);
            rightIndex = Mathf.Min(message.Length, leftIndex + _maxMessagePacketSize);

            subMessage = subMessage.Trim('\n').Replace("@n", "\n");

            RpcV3.Mass()
                .Start(sender.NetId, RpcCalls.SetName)
                .Write(title)
                .End()
                .Start(sender.NetId, RpcCalls.SendChat)
                .Write(recipient.IsModded() ? subMessage : subMessage.RemoveHtmlTags())
                .End()
                .Start(sender.NetId, RpcCalls.SetName)
                .Write(originalName)
                .End()
                .Send(recipient.GetClientId());
        }

        message = message[leftIndex..rightIndex].Trim('\n').Replace("@n", "\n");

        RpcV3.Mass()
            .Start(sender.NetId, RpcCalls.SetName)
            .Write(title)
            .End()
            .Start(sender.NetId, RpcCalls.SendChat)
            .Write(recipient.IsModded() ? message : message.RemoveHtmlTags())
            .End()
            .Start(sender.NetId, RpcCalls.SetName)
            .Write(originalName)
            .End()
            .Send(recipient.GetClientId());
    }

    // Large Title
    private static void InternalSendLT(PlayerControl sender, PlayerControl recipient, string message, string title, string originalName)
    {
        int leftIndex = 0;
        int rightIndex = Math.Min(title.Length, _maxMessagePacketSize);

        while (rightIndex < title.Length)
        {
            string subTitle = title[leftIndex..rightIndex];
            leftIndex = FindGoodSplitPoint(ref subTitle, leftIndex);
            rightIndex = Mathf.Min(title.Length, leftIndex + _maxMessagePacketSize);

            subTitle = subTitle.Trim('\n').Replace("@n", "\n");

            RpcV3.Mass()
                .Start(sender.NetId, RpcCalls.SetName)
                .Write(subTitle)
                .End()
                .Start(sender.NetId, RpcCalls.SendChat)
                .Write(recipient.IsModded() ? message : message.RemoveHtmlTags())
                .End()
                .Start(sender.NetId, RpcCalls.SetName)
                .Write(originalName)
                .End()
                .Send(recipient.GetClientId());
        }

        title = title[leftIndex..rightIndex].Trim('\n').Replace("@n", "\n");

        RpcV3.Mass()
            .Start(sender.NetId, RpcCalls.SetName)
            .Write(title)
            .End()
            .Start(sender.NetId, RpcCalls.SendChat)
            .Write(recipient.IsModded() ? message : message.RemoveHtmlTags())
            .End()
            .Start(sender.NetId, RpcCalls.SetName)
            .Write(originalName)
            .End()
            .Send(recipient.GetClientId());
    }

    private static int FindGoodSplitPoint(ref string message, int index)
    {
        int lastIndex = message.LastIndexOf("\n\n", StringComparison.Ordinal);
        if (lastIndex == -1) lastIndex = message.LastIndexOf("\n", StringComparison.Ordinal);
        if (lastIndex < (message.Length / 2)) return message.Length + index;
        message = message[..lastIndex];
        return lastIndex + index;
    }



    public class TitleBuilder
    {
        private string? text;
        private string? prefix;
        private string? suffix;

        private Color? color;

        public TitleBuilder Text(string txt)
        {
            text = txt;
            return this;
        }

        public TitleBuilder Color(Color clr)
        {
            color = clr;
            return this;
        }

        public TitleBuilder PrefixSuffix(string txt)
        {
            prefix = txt;
            suffix = txt;
            return this;
        }

        public TitleBuilder Prefix(string txt)
        {
            prefix = txt;
            return this;
        }

        public TitleBuilder Suffix(string txt)
        {
            suffix = txt;
            return this;
        }

        public string Build()
        {
            string prefix1 = prefix != null ? $"{prefix} " : "";
            string suffix1 = suffix != null ? $" {suffix}" : "";
            string wholeText = $"{prefix1}{text}{suffix1}";
            return color == null ? wholeText : color.Value.Colorize(wholeText);
        }

    }
}