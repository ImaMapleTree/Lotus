using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.GUI.Name.Impl;
using Lotus.Managers;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.NeutralKilling;
using Lotus.Extensions;
using Lotus.GUI.Name;
using Lotus.RPC;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.Neutral;

public class Copycat: CustomRole, ISabotagerRole
{
    /// <summary>
    /// A dict of role types and roles for the cat to fallback upon if the role cannot be copied properly (ex: Crewpostor bc Copycat cannot gain tasks)
    /// </summary>
    public static readonly Dictionary<Type, Func<CustomRole>> FallbackTypes = new() { {typeof(CrewPostor), () => CustomRoleManager.Static.Madmate} };
    
    private bool copyIdentity;
    private bool copyRoleProgress;
    private bool copyKillersRole;
    private bool turned;

    public bool CanSabotage() => false;

    [RoleAction(RoleActionType.Interaction)]
    private void CopycatAttacked(PlayerControl actor, Interaction interaction, ActionHandle handle)
    {
        if (turned || interaction.Intent() is not IFatalIntent) return;
        turned = true;
        if (!copyKillersRole) AssignFaction(actor.GetCustomRole().Faction);
        else AssignRole(actor);
        handle.Cancel();
    }

    [RoleAction(RoleActionType.Shapeshift)]
    private void PreventShapeshift(ActionHandle handle) => handle.Cancel();

    private void AssignRole(PlayerControl attacker)
    {
        CustomRole attackerRole = attacker.GetCustomRole();
        FallbackTypes.GetOptional(attackerRole.GetType()).IfPresent(r => attackerRole = r());
        CustomRole role = copyRoleProgress ? attackerRole : CustomRoleManager.GetCleanRole(attackerRole);
        Api.Roles.AssignRole(MyPlayer, role);
        Game.MatchData.GameHistory.AddEvent(new RoleChangeEvent(MyPlayer, role, this));

        float killCooldown = role.GetOverride(Override.KillCooldown)?.GetValue() as float? ?? AUSettings.KillCooldown();
        role.SyncOptions(new[] { new GameOptionOverride(Override.KillCooldown, killCooldown * 2) });
        Async.Schedule(() =>
        {
            MyPlayer.RpcGuardAndKill(MyPlayer);
            role.SyncOptions();
        }, NetUtils.DeriveDelay(0.05f));
        

        if (!role.GetActions(RoleActionType.Shapeshift).Any())
        {
            role.Editor = new BasicRoleEditor(role);
            role.Editor!.AddAction(this.GetActions(RoleActionType.Shapeshift).First().Item1);
        }

        if (copyIdentity) Async.Schedule(() => MyPlayer.CRpcShapeshift(attacker, false), 2);
    }

    private void AssignFaction(IFaction faction)
    {
        Faction = faction;
        RoleColor = Faction.FactionColor();
        MyPlayer.NameModel().GetComponentHolder<RoleHolder>().Add(new RoleComponent(this, GameStates.IgnStates, ViewMode.Replace, viewers: MyPlayer));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Copy Attacker's Role")
                .AddOnOffValues(false)
                .ShowSubOptionPredicate(o => (bool)o)
                .SubOption(sub2 => sub2.Name("Copy Role's Status")
                    .AddOnOffValues(false)
                    .BindBool(b => copyRoleProgress = b)
                    .Build())
                .SubOption(sub2 => sub2.Name("Copy Attacker's Identity")
                    .AddOnOffValues(false)
                    .BindBool(b => copyIdentity = b)
                    .Build())
                .BindBool(b => copyKillersRole = b)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.RoleColor(new Color(1f, 0.7f, 0.67f))
            .VanillaRole(copyKillersRole ? RoleTypes.Shapeshifter : RoleTypes.Crewmate)
            .Faction(FactionInstances.Solo)
            .SpecialType(Internals.SpecialType.Neutral);
}