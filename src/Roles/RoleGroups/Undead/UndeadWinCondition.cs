using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.Factions.Interfaces;
using TOHTOR.Factions.Undead;
using TOHTOR.Roles.RoleGroups.Undead.Roles;
using TOHTOR.Victory.Conditions;
using VentLib.Logging;

namespace TOHTOR.Roles.RoleGroups.Undead;

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