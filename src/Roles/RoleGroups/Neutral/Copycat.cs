using AmongUs.GameOptions;
using TOHTOR.API;
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
using TOHTOR.Roles.RoleGroups.Vanilla;
using TOHTOR.RPC;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace TOHTOR.Roles.RoleGroups.Neutral;

public class Copycat: CustomRole, ISabotagerRole
{
    private bool copyIdentity;
    private bool copyRoleProgress;
    private bool copyKillersRole;
    private bool turned;

    public bool CanSabotage() => false;

    [RoleAction(RoleActionType.Interaction)]
    private void CopycatAttacked(PlayerControl actor, Interaction interaction, ActionHandle handle)
    {
        if (turned) return;
        if (interaction.Intent() is not IFatalIntent) return;
        turned = true;
        if (!copyKillersRole) AssignFaction(actor.GetCustomRole().Faction);
        else Game.AssignRole(MyPlayer, actor.GetCustomRole());
        handle.Cancel();
    }

    [RoleAction(RoleActionType.Shapeshift)]
    private void PreventShapeshift(ActionHandle handle) => handle.Cancel();

    private void AssignRole(PlayerControl attacker)
    {
        CustomRole attackerRole = attacker.GetCustomRole();
        CustomRole role = copyRoleProgress ? attackerRole : CustomRoleManager.GetCleanRole(attackerRole);
        Game.AssignRole(MyPlayer, role);
        Game.GameHistory.AddEvent(new RoleChangeEvent(MyPlayer, role, this));

        if (role is Impostor impostor)
        {
            impostor.KillCooldown *= 2;
            role.SyncOptions();
            MyPlayer.RpcGuardAndKill(MyPlayer);
            impostor.KillCooldown /= 2;
            impostor.SyncOptions();
        }
        else
        {
            role.SyncOptions(new []{ new GameOptionOverride(Override.KillCooldown, AUSettings.KillCooldown() * 2)});
            MyPlayer.RpcGuardAndKill(MyPlayer);
            role.SyncOptions();
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