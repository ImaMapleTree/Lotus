using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.Factions.Crew;
using Lotus.Factions.Interfaces;
using Lotus.Roles;
using Lotus.Roles.Interfaces;
using Lotus.Extensions;

namespace Lotus.Victory.Conditions;

public class VanillaCrewmateWin: IFactionWinCondition
{
    private static readonly List<IFaction> CrewmateFaction = new() { FactionInstances.Crewmates };
    private WinReason winReason = WinReason.TasksComplete;

    public bool IsConditionMet(out List<IFaction> factions)
    {
        factions = CrewmateFaction;

        if (Game.State is not (GameState.Roaming or GameState.InMeeting)) return false;

        winReason = WinReason.TasksComplete;


        bool hasAliveEnemy = false;
        bool hasOneTaskDoer = false;
        foreach (CustomRole role in Game.GetAllPlayers().Select(p => p.GetCustomRole()))
        {
            if (role is ITaskHolderRole taskHolder && taskHolder.TasksApplyToTotal() && taskHolder.HasTasks()) hasOneTaskDoer = true;
            if (IsEligibleEnemy(role)) hasAliveEnemy = true;
            if (hasOneTaskDoer && hasAliveEnemy) break;
        }

        if (hasAliveEnemy && hasOneTaskDoer) return CheckTaskCompletion();

        winReason = WinReason.FactionLastStanding;
        return !hasAliveEnemy;
    }

    // Determines if the given role is an "enemy role"
    private static bool IsEligibleEnemy(AbstractBaseRole role)
    {
        PlayerControl player = role.MyPlayer;
        if (!player.IsAlive()) return false;
        if (role.Faction is Crewmates) return false;
        return player.GetVanillaRole().IsImpostor() && !role.RoleFlags.HasFlag(RoleFlag.CannotWinAlone);
    }

    private static bool CheckTaskCompletion()
    {
        GameData.Instance.RecomputeTaskCounts();
        return GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks;
    }

    public WinReason GetWinReason() => winReason;

    public int Priority() => -1;
}