﻿using System.Collections.Generic;
using Lotus.API.Player;
using Lotus.Chat;
using Lotus.Managers.Templates.Models.Units;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Managers.Templates.Models;

public class Template: TemplateUnit
{
    public string? Title { get; set; }
    public string? Tag { get; set; }
    public List<string>? Aliases { get; set; }
    public List<TTrigger>? Triggers { get; set; }
    public bool AliasOnly = false;

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
        if (viewer == null) Players.GetPlayers().ForEach(p => SendTo(p, data));
        else SendTo(viewer, data);
    }

    private void SendTo(PlayerControl player, object? data)
    {
        data ??= player;

        if (!Evaluate(data)) return;

        string result = Format(data);

        string title = Title != null ? Format(Title, data) : PlayerControl.LocalPlayer.name;
        ChatHandler.Of(result, title.Replace("\\n", "\n")).LeftAlign().Send(UnityOptional<PlayerControl>.Of(player));
    }

    internal void Setup()
    {
        Triggers?.ForEach(t => t.Setup(this));
    }
}