using Lotus.API.Odyssey;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using UnityEngine;
using VentLib.Utilities.Collections;

namespace Lotus.Roles.Subroles;

public class Flash: Subrole
{
    private float playerSpeedIncrease = 1f;
    private Remote<GameOptionOverride>? overrideRemote;
    
    public override string Identifier() => "◎";

    [RoleAction(RoleActionType.RoundStart)]
    private void GameStart(bool isStart)
    {
        if (!isStart) return;
        AdditiveOverride additiveOverride = new(Override.PlayerSpeedMod, playerSpeedIncrease);
        overrideRemote = Game.MatchData.Roles.AddOverride(MyPlayer.PlayerId, additiveOverride);
    }
    
    [RoleAction(RoleActionType.MyDeath)]
    private void RemoveOverride() => overrideRemote?.Delete();

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(Color.yellow);
}