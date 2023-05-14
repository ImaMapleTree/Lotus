using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Chat.Patches;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Chat;

[Localized("Announcements")]
[SuppressMessage("ReSharper", "ParameterHidesMember")]
public class ChatHandler
{
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

    public void Send() => Send(null);

    public void Send(PlayerControl? targetPlayer)
    {
        if (targetPlayer == null) targetPlayer = target;
        Send(targetPlayer, message ?? "", title, leftAligned);
    }

    public static ChatHandler Of(string? message = null, string? title = null)
    {
        ChatHandler ch = new ChatHandler();
        ch.message = message?.Replace("\\n", "\n");
        if (title != null) ch.title = title;
        return ch;
    }

    public static void Send(string message, string title) => Send(null, message, title);

    public static void Send(PlayerControl? player, string message, string? title = null, bool leftAligned = false)
    {
        Async.Schedule(() =>
        {
            PlayerControl? sender = Game.GetAlivePlayers().FirstOrDefault();
            if (sender == null) return;
        
            title ??= _defaultTitle;
        
            if (player == null) MassSend(sender, message, title, leftAligned);
            else if (player.IsHost()) SendToHost(sender, message, title, leftAligned);
            else
            {
                string name = sender.name;
                RpcV3.Mass()
                    .Start(sender.NetId, RpcCalls.SetName)
                    .Write(title)
                    .End()
                    .Start(sender.NetId, RpcCalls.SendChat)
                    .Write(player.IsModded() ? message : message.RemoveHtmlTags())
                    .End()
                    .Start(sender.NetId, RpcCalls.SetName)
                    .Write(name)
                    .End()
                    .Send(player.GetClientId());
            }
        }, 0.125f);
    }

    private static void SendToHost(PlayerControl sender, string message, string title, bool leftAligned)
    {
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
            else
            {
                RpcV3.Mass()
                    .Start(sender.NetId, RpcCalls.SetName)
                    .Write(title)
                    .End()
                    .Start(sender.NetId, RpcCalls.SendChat)
                    .Write(p.IsModded() ? message : message.RemoveHtmlTags())
                    .End()
                    .Start(sender.NetId, RpcCalls.SetName)
                    .Write(name)
                    .End()
                    .Send(p.GetClientId());
            }
        });
        
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