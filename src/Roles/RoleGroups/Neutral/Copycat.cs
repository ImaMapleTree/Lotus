using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.Factions.Interfaces;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.GUI.Name.Impl;
using TOHTOR.Managers;
using TOHTOR.Managers.History.Events;
using TOHTOR.Roles.Interactions.Interfaces;
using TOHTOR.Roles.Interfaces;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.Overrides;
using TOHTOR.Roles.RoleGroups.NeutralKilling;
using TOHTOR.RPC;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Roles.RoleGroups.Neutral;

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
            .SpecialType(SpecialType.Neutral);
}