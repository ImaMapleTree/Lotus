using Lotus.API.Odyssey;
using Lotus.Logging;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Options.Game;

namespace Lotus.Roles.Subroles;

public class Diseased: Subrole
{
    public override string Identifier() => "★";
    
    [RoleAction(RoleActionType.MyDeath)]
    private void DiseasedDies(PlayerControl killer)
    {
        Game.MatchData.Roles.AddOverride(killer.PlayerId, new MultiplicativeOverride(Override.KillCooldown, 2));
        DevLogger.Log($"Affecting: {killer.name} | {killer}");
        killer.GetCustomRole().SyncOptions();
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.42f, 0.4f, 0.16f));

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) => AddRestrictToCrew(base.RegisterOptions(optionStream));
}