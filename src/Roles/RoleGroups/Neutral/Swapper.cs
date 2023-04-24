using System.Linq;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Factions.Impostors;
using TOHTOR.Factions.Interfaces;
using TOHTOR.Factions.Neutrals;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.Options;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Roles.RoleGroups.Neutral;

public class Swapper : CustomRole
{
    private bool canTargetImpostors;
    private bool canTargetNeutrals;

    private PlayerControl? target;

    /*[DynElement(UI.Misc)]
    private string TargetDisplay() => target == null ? "" : Color.red.Colorize("Target: ") + Color.white.Colorize(target.UnalteredName());

    [RoleAction(RoleActionType.RoundStart)]
    public void RoundStart()
    {
        target = Game.GetAllPlayers().Where(p =>
        {
            if (p.PlayerId == MyPlayer.PlayerId) return false;
            IFaction faction = p.GetCustomRole().Faction;
            if (!canTargetImpostors && faction is ImpostorFaction) return false;
            return canTargetNeutrals || faction is not Solo;
        }).ToList().GetRandom();
    }


    [RoleAction(RoleActionType.OtherExiled)]
    private void CheckExecutionerWin(PlayerControl exiled)
    {
        if (target == null || target.PlayerId == exiled.PlayerId) return;
        // TODO: Add non-instant win
        //OldRPC.SwapperWin(MyPlayer.PlayerId);
    }

    [RoleAction(RoleActionType.AnyDeath)]
    private void CheckChangeRole(PlayerControl dead)
    {
        if (target == null || target.PlayerId != dead.PlayerId) return;
        target = null;
        MyPlayer.GetDynamicName().Render();
    }*/

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
                .AddOnOffValues(false).Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.RoleColor("#66E666").SpecialType(SpecialType.Neutral);
}