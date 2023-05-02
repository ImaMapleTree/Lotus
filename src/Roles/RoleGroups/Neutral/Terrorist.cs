using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.Options;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.Vanilla;
using UnityEngine;
using VentLib.Options.Game;

namespace TOHTOR.Roles.RoleGroups.Neutral;

// Inherits from crewmate because crewmate has task setup
public class Terrorist : Crewmate
{
    private bool canWinBySuicide;

    [RoleAction(RoleActionType.MyDeath)]
    private void OnTerroristDeath() => TerroristWinCheck();


    //   [RoleAction(RoleActionType.SelfExiled)]
    //  private void OnTerroristExiled() => TerroristWinCheck();

    private void TerroristWinCheck()
    {
        if (this.HasAllTasksDone)
        {
            // I know we are going to redo death reasons but I will still like it here for reasons.
            /*if (canWinBySuicide || TOHPlugin.PlayerStates[MyPlayer.PlayerId].deathReason != (PlayerStateOLD.DeathReason.Suicide | PlayerStateOLD.DeathReason.FollowingSuicide))
            {
                // TERRORIST WIN
            }*/
        }
        //OldRPC.TerroristWin(MyPlayer.PlayerId);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddTaskOverrideOptions(base.RegisterOptions(optionStream)
            .Tab(DefaultTabs.NeutralTab)
            .SubOption(sub => sub
                .Name("Can Win By Suicide")
                .Bind(v => canWinBySuicide = (bool)v)
                .AddOnOffValues(false).Build()));

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(Color.green).Faction(FactionInstances.Solo);
}