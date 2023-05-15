using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using Lotus.Logging;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.Subroles;

public class Bewilder: Subrole
{
    public override string Identifier() => "★";
    
    [RoleAction(RoleActionType.MyDeath)]
    private void BewilderDies(PlayerControl killer, Optional<PlayerControl> realKiller)
    {
        DevLogger.Log("Bewilder dies");
        if (realKiller.Exists()) killer = realKiller.Get();

        GameOptionOverride optionOverride = killer.GetVanillaRole().IsImpostor()
            ? new GameOptionOverride(Override.ImpostorLightMod, AUSettings.CrewLightMod())
            : new GameOptionOverride(Override.CrewLightMod, AUSettings.CrewLightMod() / 2);
        
        
        Game.MatchData.Roles.AddOverride(killer.PlayerId, optionOverride);
        killer.GetCustomRole().SyncOptions();
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(new Color(0.42f, 0.28f, 0.2f));

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) => AddRestrictToCrew(base.RegisterOptions(optionStream));
}