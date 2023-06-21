#nullable enable
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Factions;
using Lotus.Factions.Impostors;
using Lotus.Factions.Interfaces;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Managers;
using Lotus.Options;
using Lotus.Roles.Internals.Attributes;
using Lotus.Victory.Conditions;
using Lotus.Extensions;
using Lotus.Roles.Internals.Enums;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.Neutral;

public class Executioner : CustomRole
{
    private bool canTargetImpostors;
    private bool canTargetNeutrals;
    private int roleChangeWhenTargetDies;

    private PlayerControl? target;

    [RoleAction(RoleActionType.RoundStart)]
    private void OnGameStart(bool gameStart)
    {
        if (!gameStart) return;
        target = Players.GetPlayers().Where(p =>
        {
            if (p.PlayerId == MyPlayer.PlayerId) return false;
            IFaction faction = p.GetCustomRole().Faction;
            if (!canTargetImpostors && faction is ImpostorFaction) return false;
            return canTargetNeutrals || faction is not Factions.Neutrals.Neutral;
        }).ToList().GetRandom();
        VentLogger.Trace($"Executioner ({MyPlayer.name}) Target: {target}");

        target.NameModel().GetComponentHolder<NameHolder>().Add(new ColoredNameComponent(target, RoleColor, Game.IgnStates, MyPlayer));
    }

    [RoleAction(RoleActionType.AnyExiled)]
    private void CheckExecutionerWin(GameData.PlayerInfo exiled)
    {
        if (target == null || target.PlayerId != exiled.PlayerId) return;
        List<PlayerControl> winners = new() { MyPlayer };
        if (target.GetCustomRole() is Jester) winners.Add(target);
        ManualWin win = new(winners, ReasonType.SoloWinner);
        win.Activate();
    }

    [RoleAction(RoleActionType.Disconnect)]
    [RoleAction(RoleActionType.AnyDeath)]
    private void CheckChangeRole(PlayerControl dead)
    {
        if (roleChangeWhenTargetDies == 0 || target == null || target.PlayerId != dead.PlayerId) return;
        switch ((ExeRoleChange)roleChangeWhenTargetDies)
        {
            case ExeRoleChange.Jester:
                MatchData.AssignRole(MyPlayer, CustomRoleManager.Static.Jester);
                break;
            case ExeRoleChange.Opportunist:
                MatchData.AssignRole(MyPlayer, CustomRoleManager.Static.Opportunist);
                break;
            case ExeRoleChange.SchrodingersCat:
                MatchData.AssignRole(MyPlayer, CustomRoleManager.Static.SchrodingersCat);
                break;
            case ExeRoleChange.Crewmate:
                MatchData.AssignRole(MyPlayer, CustomRoleManager.Static.Crewmate);
                break;
            case ExeRoleChange.None:
            default:
                break;
        }

        target = null;
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .Tab(DefaultTabs.NeutralTab)
            .SubOption(sub => sub
                .Name("Can Target Impostors")
                .Bind(v => canTargetImpostors = (bool)v)
                .AddOnOffValues(false).Build())
            .SubOption(sub => sub
                .Name("Can Target Neutrals")
                .Bind(v => canTargetNeutrals = (bool)v)
                .AddOnOffValues(false).Build())
            .SubOption(sub => sub
                .Name("Role Change When Target Dies")
                .Bind(v => roleChangeWhenTargetDies = (int)v)
                .Value(v => v.Text("Jester").Value(1).Color(new Color(0.93f, 0.38f, 0.65f)).Build())
                .Value(v => v.Text("Opportunist").Value(2).Color(Color.green).Build())
                .Value(v => v.Text("Copycat").Value(3).Color(new Color(1f, 0.7f, 0.67f)).Build())
                .Value(v => v.Text("Crewmate").Value(4).Color(new Color(0.71f, 0.94f, 1f)).Build())
                .Value(v => v.Text("Off").Value(0).Color(Color.red).Build())
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.RoleColor(new Color(0.55f, 0.17f, 0.33f)).Faction(FactionInstances.Neutral).RoleFlags(RoleFlag.CannotWinAlone).SpecialType(SpecialType.Neutral);

    private enum ExeRoleChange
    {
        None,
        Jester,
        Opportunist,
        SchrodingersCat,
        Crewmate
    }
}