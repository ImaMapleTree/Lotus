using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.Overrides;
using UnityEngine;
using VentLib.Options.Game;

namespace TOHTOR.Roles.Subroles;

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