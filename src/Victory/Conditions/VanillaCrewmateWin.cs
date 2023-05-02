using System.Collections.Generic;
using System.Linq;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.Factions.Crew;
using TOHTOR.Factions.Interfaces;
using TOHTOR.Roles;
using TOHTOR.Roles.Interfaces;
using VentLib.Logging;

namespace TOHTOR.Victory.Conditions;

public class VanillaCrewmateWin: IFactionWinCondition
{
    private static readonly List<IFaction> CrewmateFaction = new() { FactionInstances.Crewmates };
    private WinReason winReason = WinReason.TasksComplete;

    public bool IsConditionMet(out List<IFaction> factions)
    {
        factions = CrewmateFaction;

        if (Game.State is not GameState.Roaming) return false;

        winReason = WinReason.TasksComplete;


        bool hasAliveImpostor = false;
        bool hasOneTaskDoer = false;
        foreach (CustomRole role in Game.GetAllPlayers().Select(p => p.GetCustomRole()))
        {
            if (role is ITaskHolderRole taskHolder && taskHolder.TasksApplyToTotal() && taskHolder.HasTasks()) hasOneTaskDoer = true;

            if (role.MyPlayer.IsAlive() && role.Faction is not Crewmates && Game.VanillaRoleTracker.GetInfo(role.MyPlayer.PlayerId).MyRole.IsImpostor()) hasAliveImpostor = true;
            if (hasOneTaskDoer && hasAliveImpostor) break;
        }

        if (hasAliveImpostor && hasOneTaskDoer) return CheckTaskCompletion();

        winReason = WinReason.FactionLastStanding;
        return !hasAliveImpostor;
    }

    private static bool CheckTaskCompletion()
    {
        GameData.Instance.RecomputeTaskCounts();
        return GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks;
    }

    public WinReason GetWinReason() => winReason;

    public int Priority() => -1;
}