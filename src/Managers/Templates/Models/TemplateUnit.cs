using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AmongUs.Data;
using InnerNet;
using Lotus.API.Odyssey;
using Lotus.Chat;
using Lotus.Extensions;
using Lotus.Roles;
using Lotus.Roles.Subroles;
using VentLib.Options;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers.Templates.Models;

public class TemplateUnit
{
    private static readonly Regex Regex = new("(?:\\$|@|%|\\^)((?:[A-Za-z0-9_]|\\.\\S)*)");
    public string? Text { get; set; }
    public TCondition? Condition { get; set; }

    public string Format(PlayerControl? user, object? obj = null)
    {
        obj ??= user;
        if (Text == null) return "";

        // ReSharper disable once InvertIf
        if (Condition != null)
        {
            if (!Condition.VerifyRole(user)) return "";
            if (!Condition.VerifyStatus(user)) return "";
        }


        return Regex.Replace(Text, match =>
        {
            var split = match.Value.Split(".");
            if (split.Length > 1)
                return VariableValues.TryGetValue(split[0], out var dynSupplier)
                    ? dynSupplier(user, obj!, split[1])
                    : match.Value;
            return TemplateValues.TryGetValue(match.Value, out var funcSupplier)
                ? funcSupplier(obj!)
                : PluginDataManager.TemplateManager.FormatVariable(split[0][1..], user, obj) ?? match.Value;
        });
    }


    public static readonly Dictionary<string, Func<object, String>> TemplateValues = new()
    {
        { "$RoomCode", _ => GameCode.IntToGameName(AmongUsClient.Instance.GameId) },
        { "$Host", _ => DataManager.Player.Customization.name },
        { "$AUVersion", _ => UnityEngine.Application.version },
        { "$ModVersion", _ => ProjectLotus.PluginVersion + (ProjectLotus.DevVersion ? " " + ProjectLotus.DevVersionStr : "") },
        { "$Map", _ => Constants.MapNames[GameOptionsManager.Instance.CurrentGameOptions.MapId] },
        { "$Gamemode", _ => Game.CurrentGamemode.GetName() },
        { "$Date", _ => DateTime.Now.ToShortDateString() },
        { "$Time", _ => DateTime.Now.ToShortTimeString() },
        { "$Players", _ => PlayerControl.AllPlayerControls.ToArray().Select(p => p.name).Fuse() },
        { "$PlayerCount", _ => PlayerControl.AllPlayerControls.Count.ToString() },
        { "@Name", player => ((PlayerControl) player).name },
        { "@Color", player => ModConstants.ColorNames[((PlayerControl) player).cosmetics.bodyMatProperties.ColorId] },
        { "@Role", player =>
            {
                CustomRole role = ((PlayerControl)player).GetCustomRole();
                return role.RoleColor.Colorize(role.RoleName);
            }
        },
        { "@Blurb", player => ((PlayerControl) player).GetCustomRole().Blurb },
        { "@Description", player => ((PlayerControl) player).GetCustomRole().Description },
        { "@Options", player => OptionUtils.OptionText(((PlayerControl) player).GetCustomRole().RoleOptions) },
        { "@Faction", player => ((PlayerControl) player).GetCustomRole().Faction.Name() },
        { "@Subroles", player => ((PlayerControl) player).GetSubroles().Select(r => r.RoleColor.Colorize(r.RoleName)).Fuse() },
        { "@Modifiers", player => ((PlayerControl) player).GetSubroles().Select(r => r.RoleColor.Colorize(r.RoleName)).Fuse() },
        { "@Mods", player => ((PlayerControl) player).GetSubroles().Select(r => r.RoleColor.Colorize(r.RoleName)).Fuse() },
        { "@ModsDescriptive", player => ModifierText((PlayerControl) player) },
        { "^Role_Name", role => ((CustomRole) role).RoleName },
        { "^Role_Description", role => ((CustomRole) role).Description },
        { "^Role_Blurb", role => ((CustomRole) role).Blurb },
        { "^Role_Options", role => OptionUtils.OptionText(((CustomRole) role).RoleOptions) },
        { "^Role_Faction", role => ((CustomRole) role).Faction.Name() },
        { "^Role_Basis", role => ((CustomRole) role).RealRole.ToString() }
    };

    public static readonly Dictionary<string, TFormat> VariableValues = new()
    {
        { "%Option", (_, _, qualifier) => OptionManager.GetManager().GetOption(qualifier)?.GetValueText() ?? "Unknown Option" },
        { "%Template", (player, obj, template) => PluginDataManager.TemplateManager.TryFormat(player, obj, template, out string text) ? text : "" }
    };

    private static string ModifierText(PlayerControl player)
    {
        if (PluginDataManager.TemplateManager.HasTemplate("modifier-info"))
        {
            return player.GetSubroles().Select(sr => !PluginDataManager.TemplateManager.TryFormat(player, sr, "modifier-info", out string text) ? "" : text).Fuse("\n\n");
        }

        return player.GetSubroles().Select(sr =>
        {
            string identifierText = sr is Subrole subrole ? sr.RoleColor.Colorize(subrole.Identifier()) + " " : "";
            return $"{identifierText}{sr.RoleColor.Colorize(sr.RoleName)}\n{sr.Description}";
        }).Fuse("\n\n");
    }

    // ReSharper disable once InconsistentNaming
    public delegate string TFormat(PlayerControl? player, object? obj, string context);
}