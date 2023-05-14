using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.Roles.Internals.Attributes;
using Lotus.API;
using Lotus.GUI;
using Lotus.GUI.Name;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Creeper : Vanilla.Impostor // hidden role. 5% chance to replace Bomber. so no options for this role.
{
    private bool exploded;
    [RoleAction(RoleActionType.OnPet)]
    private void Explode()
    {
        if (exploded || MyPlayer.Data.IsDead) return;
        exploded = true;
        List<PlayerControl> allPlayersInDistance = RoleUtils.GetPlayersWithinDistance(MyPlayer.GetTruePosition(), 3f).Distinct().ToList();
        // we can add an explode time if you want. . .
        allPlayersInDistance.Add(MyPlayer); // they will explode along with everyone else.
        allPlayersInDistance.Distinct().Do(explodedPlayer => {
            explodedPlayer.RpcMurderPlayer(explodedPlayer);
        });
    }
    protected override RoleModifier Modify(RoleModifier roleModifier)
    {
        base.Modify(roleModifier);
        //VentLogger.Warn($"{this.RoleName} Not Implemented Yet", "RoleImplementation");
        return roleModifier;
    }
}