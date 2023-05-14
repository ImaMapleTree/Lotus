using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.Factions.Undead;
using Lotus.Roles.RoleGroups.Undead.Roles;
using Lotus.Victory.Conditions;
using Lotus.API;
using Lotus.Extensions;
using VentLib.Logging;

namespace Lotus.Roles.RoleGroups.Undead;

public class UndeadWinCondition : IFactionWinCondition
{
    private static readonly List<IFaction> UndeadFactions = new() { FactionInstances.TheUndead, new TheUndead.Unconverted(null!, null!) };
    public bool IsConditionMet(out List<IFaction> factions)
    {
        factions = UndeadFactions;

        int aliveUndead = 0;
        int aliveOther = 0;

        bool necromancerAlive = false;
        foreach (CustomRole role in Game.GetAlivePlayers().Select(p => p.GetCustomRole()))
        {
            if (role is Necromancer) necromancerAlive = true;
            if (role.Faction is TheUndead) aliveUndead++;
            else aliveOther++;
        }

        //if (necromancerAlive && aliveUndead >= aliveOther) VentLogger.Info("Undead Win");
        return necromancerAlive && aliveUndead >= aliveOther;
    }

    public WinReason GetWinReason() => WinReason.FactionLastStanding;
}