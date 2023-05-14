using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Options.Game;

namespace Lotus.Roles.Subroles;

public class Bewilder: Subrole
{
    public override string Identifier() => "★";
    
    [RoleAction(RoleActionType.MyDeath)]
    private void BaitDies(PlayerControl killer)
    {
        Game.MatchData.Roles.AddOverride(killer.PlayerId, new GameOptionOverride(Override.ImpostorLightMod, AUSettings.CrewLightMod()));
        killer.GetCustomRole().SyncOptions();
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(new Color(0.42f, 0.28f, 0.2f));

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) => AddRestrictToCrew(base.RegisterOptions(optionStream));
}