/*using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.Extensions;

namespace TOHTOR.Factions;

public enum FactionOld: ulong
{
    Crewmates = 0,
    Solo = 1,
    Impostors = 2,
    Coven = 3,
    UndeadConverting = 4,
    Undead,
    Madmate
}

public static class FactionMethods
{
    public static bool IsAllied(this FactionOld factionOld, FactionOld[] factions)
    {
        if (factionOld is FactionOld.Solo or FactionOld.Madmate) return false;
        return !factions.Contains(FactionOld.Solo) && factions.Contains(factionOld);
    }

    public static bool IsAllied(this FactionOld factionOld, FactionOld otherFactionOld)
    {
        if (factionOld is FactionOld.Solo or FactionOld.Madmate || otherFactionOld is FactionOld.Solo or FactionOld.Madmate) return false;
        return factionOld == otherFactionOld;
    }

    public static bool IsAllied(this IEnumerable<FactionOld> factions, FactionOld other)
    {
        IEnumerable<FactionOld> enumerable = factions as FactionOld[] ?? factions.ToArray();
        return other is not (FactionOld.Solo or FactionOld.Madmate) && !(enumerable.Contains(FactionOld.Solo) || enumerable.Contains(FactionOld.Madmate)) && enumerable.Contains(other);
    }

    public static bool IsAllied(this IEnumerable<FactionOld> factions, IEnumerable<FactionOld> others)
    {
        IEnumerable<FactionOld> enumerableThis = factions as FactionOld[] ?? factions.ToArray();
        IEnumerable<FactionOld> enumerableOthers = factions as FactionOld[] ?? others.ToArray();
        return !(enumerableThis.Contains(FactionOld.Solo) || enumerableThis.Contains(FactionOld.Madmate)) && !(enumerableOthers.Contains(FactionOld.Solo) || enumerableOthers.Contains(FactionOld.Madmate)) && (enumerableThis.Any(f => enumerableOthers.Contains(f) || enumerableOthers.Any(f => enumerableThis.Contains(f))));
    }

    public static bool IsSolo(this IEnumerable<FactionOld> factions) => factions.Contains(FactionOld.Solo);
    public static bool IsImpostor(this IEnumerable<FactionOld> factions) => factions.Contains(FactionOld.Impostors);
    public static bool IsCrewmate(this IEnumerable<FactionOld> factions) => factions.Contains(FactionOld.Crewmates);
    public static bool IsUndead(this IEnumerable<FactionOld> factions) => factions.Contains(FactionOld.Undead);

    public static List<PlayerControl> GetAllies(this IEnumerable<FactionOld> factions) => Game.GetAllPlayers().Where(p => factions.IsAllied(p.GetCustomRole().FactionsOld)).ToList();
}*/