using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AmongUs.Data;
using HarmonyLib;
using InnerNet;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using VentLib.Options;

namespace TOHTOR.Managers.Templates;

public class Template
{
    private static readonly Regex Regex = new("(?:\\$|@|%)((?:[A-Za-z0-9]|\\.\\S)*)");

    public string Text { get; set; } = "";
    public HashSet<string> Profiles { get; set; } = new();

    public Template()
    {
    }

    public Template(string text)
    {
        Text = text;
        Profiles.Add(PluginDataManager.TemplateManager.GetProfile());
    }

    public string Format(PlayerControl player)
    {
        return Regex.Replace(Text, match =>
        {
            var split = match.Value.Split(".");
            if (split.Length > 1)
                return VariableValues.TryGetValue(split[0], out var dynSupplier)
                    ? dynSupplier(player, split[1])
                    : match.Value;
            return TemplateValues.TryGetValue(match.Value, out var funcSupplier)
                ? funcSupplier(player)
                : match.Value;
        });
    }

    private static readonly Dictionary<string, Func<PlayerControl, String>> TemplateValues = new()
    {
        { "$RoomCode", _ => GameCode.IntToGameName(AmongUsClient.Instance.GameId) },
        { "$Host", _ => DataManager.Player.Customization.name },
        { "$AUVersion", _ => UnityEngine.Application.version },
        { "$ModVersion", _ => TOHPlugin.PluginVersion + (TOHPlugin.DevVersion ? " " + TOHPlugin.DevVersionStr : "") },
        { "$Map", _ => Constants.MapNames[GameOptionsManager.Instance.CurrentGameOptions.MapId] },
        { "$Gamemode", _ => Game.CurrentGamemode.GetName() },
        { "$Date", _ => DateTime.Now.ToShortDateString() },
        { "$Time", _ => DateTime.Now.ToShortTimeString() },
        { "$Players", _ => PlayerControl.AllPlayerControls.ToArray().Select(p => p.name).Join() },
        { "$PlayerCount", _ => PlayerControl.AllPlayerControls.Count.ToString() },
        { "@Name", player => player.name },
        { "@Color", player => ModConstants.ColorNames[player.cosmetics.bodyMatProperties.ColorId] },
        { "@Role", player => player.GetCustomRole().RoleName },
        { "@Blurb", player => player.GetCustomRole().Blurb },
        { "@Description", player => player.GetCustomRole().Description },
        { "%CEnd", _ => "</color>"}
    };

    private static readonly Dictionary<string, Func<PlayerControl, string, string>> VariableValues = new()
    {
        { "%Option", (_, qualifier) => OptionManager.GetManager().GetOption(qualifier)?.GetValueText() ?? "Unknown Option" },
    };

}