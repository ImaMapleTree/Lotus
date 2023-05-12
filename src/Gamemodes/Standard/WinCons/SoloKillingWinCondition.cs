using System.Collections.Generic;
using System.Linq;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.Factions.Neutrals;
using TOHTOR.Logging;
using TOHTOR.Roles;
using TOHTOR.Victory.Conditions;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Gamemodes.Standard.WinCons;

public class SoloKillingWinCondition : IWinCondition
{
    public bool IsConditionMet(out List<PlayerControl> winners)
    {
        winners = null;
        
        int alivePlayers = 0;
        List<CustomRole> aliveKillers = new();

        Game.GetAliveRoles().ForEach(r => {
            alivePlayers++;
            if (r.RoleFlags.HasFlag(RoleFlag.CannotWinAlone)) return;
            if (r.Faction is not Solo) return;
            if (!r.MyPlayer.GetVanillaRole().IsImpostor()) return;
           aliveKillers.Add(r);
        });

        if (alivePlayers - aliveKillers.Count > 1 || aliveKillers.Count == 0) return false;
        
        foreach (CustomRole killer in aliveKillers)
        {
            foreach (CustomRole killer2 in aliveKillers.Where(k => k.MyPlayer.PlayerId != killer.MyPlayer.PlayerId))
            {
                DevLogger.Log($"Relationship of {killer.MyPlayer.name} to {killer2.MyPlayer.name} => {killer.Relationship(killer2)}");
                if (killer.Relationship(killer2) is Relation.None) return false;
            }
        }

        winners = aliveKillers.Select(k => k.MyPlayer).ToList();
        return true;
    }

    public WinReason GetWinReason() => WinReason.SoloWinner;
}