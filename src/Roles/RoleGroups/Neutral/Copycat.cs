using AmongUs.GameOptions;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.Factions.Interfaces;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.GUI.Name.Impl;
using TOHTOR.Roles.Interactions.Interfaces;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using UnityEngine;
using VentLib.Options.Game;

namespace TOHTOR.Roles.RoleGroups.Neutral;

// TODO: fix role copying
public class Copycat: CustomRole
{
    private bool copyKillersRole;
    private bool turned;

    [RoleAction(RoleActionType.Interaction)]
    private void TricksterTurned(PlayerControl actor, Interaction interaction, ActionHandle handle)
    {
        if (turned) return;
        if (interaction.Intent() is not IFatalIntent) return;
        turned = true;
        if (copyKillersRole) Game.AssignRole(MyPlayer, actor.GetCustomRole());
        else AssignFaction(actor.GetCustomRole().Faction);
        handle.Cancel();
    }

    private void AssignFaction(IFaction faction)
    {
        Faction = faction;
        RoleColor = Faction.FactionColor();
        MyPlayer.NameModel().GetComponentHolder<RoleHolder>().Add(new RoleComponent(this, GameStates.IgnStates, ViewMode.Replace, viewers: MyPlayer));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Copy Killers Role")
                .AddOnOffValues(false)
                .BindBool(b => copyKillersRole = b)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.RoleColor(new Color(1f, 0.7f, 0.67f))
            .VanillaRole(copyKillersRole ? RoleTypes.Impostor : RoleTypes.Crewmate)
            .Faction(FactionInstances.Solo)
            .SpecialType(SpecialType.Neutral);
}