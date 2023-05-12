using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AmongUs.Data;
using HarmonyLib;
using InnerNet;
using TOHTOR.API.Odyssey;
using TOHTOR.Chat;
using TOHTOR.Extensions;
using TOHTOR.Roles;
using VentLib.Options;

namespace TOHTOR.Managers.Templates;

public class Template
{
    private static readonly Regex Regex = new("(?:\\$|@|%|\\^)((?:[A-Za-z0-9_]|\\.\\S)*)");

    public string Text;
    public string? Tag;
    
    public Template(string text)
    {
        Text = text.Replace("\\n", "\n");
        text = text.Replace("\\|", "\\?");
        if (!text.Contains(" | ")) return;
        
        string[] split = text.Split(" | ");
        Tag = split[0];
        Text = split[1].Replace("\\?", "|");
    }

    public string Format(object obj)
    {
        return Regex.Replace(Text, match =>
        {
            var split = match.Value.Split(".");
            if (split.Length > 1)
                return VariableValues.TryGetValue(split[0], out var dynSupplier)
                    ? dynSupplier(obj, split[1])
                    : match.Value;
            return TemplateValues.TryGetValue(match.Value, out var funcSupplier)
                ? funcSupplier(obj)
                : match.Value;
        });
    }

    private static readonly Dictionary<string, Func<object, String>> TemplateValues = new()
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
        { "@Name", player => ((PlayerControl) player).name },
        { "@Color", player => ModConstants.ColorNames[((PlayerControl) player).cosmetics.bodyMatProperties.ColorId] },
        { "@Role", player => ((PlayerControl) player).GetCustomRole().RoleName },
        { "@Blurb", player => ((PlayerControl) player).GetCustomRole().Blurb },
        { "@Description", player => ((PlayerControl) player).GetCustomRole().Description },
        { "@Options", player => OptionUtils.OptionText(((PlayerControl) player).GetCustomRole().Options) },
        { "@Faction", player => ((PlayerControl) player).GetCustomRole().Faction.Name() },
        { "^Role_Name", role => ((CustomRole) role).RoleName },
        { "^Role_Description", role => ((CustomRole) role).Description },
        { "^Role_Blurb", role => ((CustomRole) role).Blurb },
        { "^Role_Options", role => OptionUtils.OptionText(((CustomRole) role).Options) },
        { "^Role_Faction", role => ((CustomRole) role).Faction.Name() },
        { "^Role_Basis", role => ((CustomRole) role).RealRole.ToString() }
    };

    public static readonly Dictionary<string, string> TemplateVariables = new()
    {
        { "$RoomCode", "The current room code." },
        { "$Host", "The host's name." },
        { "$AUVersion", "The current version of Among Us." },
        { "$ModVersion", "The current mod version." },
        { "$Map", "The current map name." },
        { "$Gamemode", "The current gamemode name." },
        { "$Date", "The current date (based on the host)." },
        { "$Time", "The current time (based on the host)." },
        { "$Players", "A list of all player names separated by a comma." },
        { "$PlayerCount", "A count of all players currently in the lobby." },
        { "@Name", "The previewer's name." },
        { "@Color", "The previewer's color." },
        { "@Role", "The previewer's role." },
        { "@Blurb", "The previewer's role blurb." },
        { "@Description", "The previewer's role description." },
        { "@Options", "The previewer's role options." },
        { "@Faction", "The previewer's faction." },
        { "^Role_Name", "The name of the role." },
        { "^Role_Description", "The description of the role." },
        { "^Role_Blurb", "The blurb for the role." },
        { "^Role_Options", "The current options for the role." },
        { "^Role_Faction", "The faction of the role." },
        { "^Role_Basis", "The role's (vanilla) basis." }
    };

    private static readonly Dictionary<string, Func<object, string, string>> VariableValues = new()
    {
        { "%Option", (_, qualifier) => OptionManager.GetManager().GetOption(qualifier)?.GetValueText() ?? "Unknown Option" },
    };

}