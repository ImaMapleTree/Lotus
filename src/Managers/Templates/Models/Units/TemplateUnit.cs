﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AmongUs.Data;
using InnerNet;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Chat;
using Lotus.Chat.Commands;
using Lotus.Extensions;
using Lotus.Factions.Interfaces;
using Lotus.Managers.Templates.Models.Backing;
using Lotus.Roles;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Subroles;
using VentLib.Options;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Managers.Templates.Models.Units;

public class TemplateUnit
{
    public static byte Triggerer = byte.MaxValue;
    protected static string? MetaVariable;

    private static readonly Regex Regex = new(@"(?:\$)\{((?:[A-Za-z0-9_ ]|\.\S)*)\}");
    public string? Text { get; set; }
    public List<TCondition> Conditions { get; set; } = new();
    public List<TCondition> ConditionsAny { get; set; } = new();

    protected bool Evaluate(object? data)
    {
        return Conditions.All(c => c.Evaluate(data)) || ConditionsAny.Any(c => c.Evaluate(data));
    }

    public string Format(object? obj = null) => Text == null ? "" : Format(Text, obj);

    public string Format(string text, object? obj = null) => Evaluate(obj) ? FormatStatic(text, obj) : (Conditions.FirstOrDefault(c => c.Fallback != null)?.Fallback ?? "");

    public static string FormatStatic(string text, object? obj = null)
    {
        return Regex.Replace(text, match =>
        {
            string value = match.Groups[1].Value;
            var split = value.Split(".");
            if (split.Length > 1)
                return VariableValues.TryGetValue(split[0], out var dynSupplier)
                    ? dynSupplier(obj!, split[1..].Fuse("."))
                    : match.Value;
            return TemplateValues.TryGetValue(value, out var funcSupplier)
                ? funcSupplier(obj!)
                : PluginDataManager.TemplateManager.FormatVariable(value, obj) ?? match.Value;
        }).Replace("\n", "@n");
    }

    public static readonly Dictionary<string, Func<object, String>> TemplateValues = new()
    {
        { "RoomCode", _ => GameCode.IntToGameName(AmongUsClient.Instance.GameId) },
        { "Host", _ => DataManager.Player.Customization.name },
        { "AUVersion", _ => UnityEngine.Application.version },
        { "ModName" , _ => ProjectLotus.ModName },
        { "ModVersion", _ => ProjectLotus.PluginVersion + (ProjectLotus.DevVersion ? " " + ProjectLotus.DevVersionStr : "") },
        { "Map", _ => Constants.MapNames[GameOptionsManager.Instance.CurrentGameOptions.MapId] },
        { "Gamemode", _ => Game.CurrentGamemode.GetName() },
        { "Date", _ => DateTime.Now.ToShortDateString() },
        { "Time", _ => DateTime.Now.ToShortTimeString() },
        { "Players", _ => PlayerControl.AllPlayerControls.ToArray().Select(p => p.name).Fuse() },
        { "PlayerCount", _ => PlayerControl.AllPlayerControls.Count.ToString() },
        { "AllRoles", _ => TUAllRoles.GetAllRoles(false) },
        { "AllModifiers", _ => TUAllRoles.GetAllRoles(true, true).Replace("Modifiers\n", "") },

        { "AlivePlayers", _ => Players.GetPlayers(PlayerFilter.Alive).Select(p => p.name).Fuse() },
        { "AlivePlayerCount", _ => Players.GetPlayers(PlayerFilter.Alive).Count().ToString() },
        { "DeadPlayers", _ => Players.GetPlayers(PlayerFilter.Dead).Select(p => p.name).Fuse() },
        { "DeadPlayerCount", _ => Players.GetPlayers(PlayerFilter.Dead).Count().ToString() },

        { "Impostors", _ => Players.GetPlayers(PlayerFilter.Impostor).Select(p => p.name).Fuse() },
        { "ImpostorsCount", _ => Players.GetPlayers(PlayerFilter.Impostor).Count().ToString() },
        { "AliveImpostorsCount", _ => Players.GetPlayers(PlayerFilter.Alive | PlayerFilter.Impostor).Count().ToString() },

        { "Crewmates", _ => Players.GetPlayers(PlayerFilter.Crewmate).Select(p => p.name).Fuse().ToString() },
        { "CrewmatesCount", _ => Players.GetPlayers(PlayerFilter.Crewmate).Count().ToString() },
        { "AliveCrewmatesCount", _ => Players.GetPlayers(PlayerFilter.Alive | PlayerFilter.Crewmate).Count().ToString() },

        { "Neutrals", _ => Players.GetPlayers(PlayerFilter.Neutral).Select(p => p.name).Fuse().ToString() },
        { "NeutralsCount", _ => Players.GetPlayers(PlayerFilter.Neutral).Count().ToString() },
        { "AliveNeutralsCount", _ => Players.GetPlayers(PlayerFilter.Alive | PlayerFilter.Neutral).Count().ToString() },

        { "NeutralKillers", _ => Players.GetPlayers(PlayerFilter.NeutralKilling).Select(p => p.name).Fuse().ToString() },
        { "NeutralKillersCount", _ => Players.GetPlayers(PlayerFilter.NeutralKilling).Count().ToString() },
        { "AliveNeutralKillersCount", _ => Players.GetPlayers(PlayerFilter.Alive | PlayerFilter.NeutralKilling).Count().ToString() },

        { "Name", player => ((PlayerControl) player).name },
        { "Level", player => ((PlayerControl) player).Data.PlayerLevel.ToString() },
        { "Color", player => ModConstants.ColorNames[((PlayerControl) player).cosmetics.bodyMatProperties.ColorId] },
        { "Role", player =>((PlayerControl)player).GetCustomRole().ColoredRoleName() },
        { "Blurb", player => ((PlayerControl) player).GetCustomRole().Blurb },
        { "Description", player => ((PlayerControl) player).GetCustomRole().Description },
        { "Status", player => Optional<FrozenPlayer>.Of(Game.MatchData.FrozenPlayer((PlayerControl)player)).Map(StatusCommand.GetPlayerStatus).OrElse("")},
        { "Death", player => Game.MatchData.GameHistory.GetCauseOfDeath(((PlayerControl)player).PlayerId).Map(c => c.SimpleName()).OrElse("Unknown") },
        { "Killer", player => Game.MatchData.GameHistory.GetCauseOfDeath(((PlayerControl)player).PlayerId).FlatMap(c => c.Instigator()).Map(p => p.Name).OrElse("Unknown") },
        { "Options", player => OptionUtils.OptionText(((PlayerControl) player).GetCustomRole().RoleOptions) },
        { "Faction", player =>
            {
                IFaction faction = ((PlayerControl)player).GetCustomRole().Faction;
                return faction.Color.Colorize(faction.Name());
            }
        },
        { "Modifiers", ShowModifiers },
        { "Mods", ShowModifiers },
        { "ModsDescriptive", ModifierText },
        { "MyRole", player => MyRoleCommand.GenerateMyRoleText(((PlayerControl)player).GetCustomRole()) },
        { "TasksComplete", QW(p => (p.GetCustomRole() is ITaskHolderRole tr ? tr.CompleteTasks : -1).ToString() )},
        { "TotalTasks", QW(p => (p.GetCustomRole() is ITaskHolderRole tr ? tr.TotalTasks : -1).ToString() )},
        { "TasksRemaining", QW(p => (p.GetCustomRole() is ITaskHolderRole tr ? tr.TotalTasks - tr.CompleteTasks : -1).ToString())},

        { "Role_Name", role => ((CustomRole) role).RoleName },
        { "Role_Description", role => ((CustomRole) role).Description },
        { "Role_Blurb", role => ((CustomRole) role).Blurb },
        { "Role_Options", role => OptionUtils.OptionText(((CustomRole) role).RoleOptions) },
        { "Role_Faction", role => ((CustomRole) role).Faction.Name() },
        { "Role_Basis", role => ((CustomRole) role).RealRole.ToString() },

        {"TriggerMeta", _ => MetaVariable ?? "" },
        {"Triggerer", _ => Players.FindPlayerById(Triggerer)?.name ?? "Unknown"}
    };

    private static string ShowModifiers(object obj)
    {
        return ((PlayerControl) obj).GetSubroles().OrderBy(r => r.RoleName).Select(r => r.ColoredRoleName()).Fuse();
    }

    public static readonly Dictionary<string, TFormat> VariableValues = new()
    {
        { "Option", (_, qualifier) => OptionManager.GetManager().GetOption(qualifier)?.GetValue().ToString() ?? $"Unknown Option \"{qualifier}\"" },
        { "Optionf", (_, qualifier) => OptionManager.GetManager().GetOption(qualifier)?.GetValueText() ?? $"Unknown Option \"{qualifier}\"" },
        { "OptionName", (_, qualifier) => OptionManager.GetManager().GetOption(qualifier)?.Name() ?? $"Unknown Option \"{qualifier}\"" },
        { "Template", (obj, template) => PluginDataManager.TemplateManager.TryFormat(obj, template, out string text) ? text : "" },
    };

    private static string ModifierText(object obj)
    {
        PlayerControl player = (PlayerControl)obj;
        IEnumerable<CustomRole> subroles = player.GetSubroles().OrderBy(r => r.RoleName);
        if (PluginDataManager.TemplateManager.HasTemplate("modifier-info"))
            return subroles.Select(sr => !PluginDataManager.TemplateManager.TryFormat(sr, "modifier-info", out string text) ? "" : text).Fuse("\n\n");

        return subroles.Select(sr =>
        {
            string identifierText = sr is Subrole subrole ? sr.RoleColor.Colorize(subrole.Identifier()) + " " : "";
            return $"{identifierText}{sr.RoleColor.Colorize(sr.RoleName)}\n{sr.Description}";
        }).Fuse("\n\n");
    }

    private static Func<object, string> QW(Func<PlayerControl, string> playerToString) => obj => playerToString((PlayerControl)obj);

    // ReSharper disable once InconsistentNaming
    public delegate string TFormat(object? obj, string context);


}