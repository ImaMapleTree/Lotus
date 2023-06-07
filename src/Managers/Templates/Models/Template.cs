using System;
using System.Collections.Generic;
using Lotus.API.Player;
using Lotus.Chat;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers.Templates.Models;

public class Template: TemplateUnit
{
    public string? Title { get; set; }
    public string? Tag { get; set; }
    public List<string>? Aliases { get; set; }

    public Template()
    {
    }

    public Template(string message, string? tag = null, string? title = null)
    {
        Text = message;
        Tag = tag;
        Title = title;
    }

    public void SendMessage(PlayerControl user, PlayerControl? viewer, object? data = null)
    {
        if (Condition != null)
        {
            if (!Condition.VerifyUser(user)) return;
            if (!Condition.VerifyUnspecific()) return;
        }

        if (viewer == null) Players.GetAllPlayers().ForEach(p => SendTo(p, data));
        else SendTo(viewer, data);
    }

    private void SendTo(PlayerControl player, Object? data)
    {
        if (Condition != null)
        {
            if (!Condition.VerifyRole(player)) return;
            if (!Condition.VerifyStatus(player)) return;
        }

        data ??= player;
        string result = Format(player, data);

        ChatHandler.Of(result, Title ?? PlayerControl.LocalPlayer.name).LeftAlign().Send(player);
    }
}