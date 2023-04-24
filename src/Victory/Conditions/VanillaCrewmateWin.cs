using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.Factions.Crew;
using TOHTOR.Factions.Interfaces;
using TOHTOR.FactionsOLD;
using TOHTOR.Roles;

namespace TOHTOR.Victory.Conditions;

public class VanillaCrewmateWin: IFactionWinCondition
{
    private static readonly List<IFaction> CrewmateFaction = new() { FactionInstances.Crewmates };
    private WinReason winReason = WinReason.TasksComplete;

    public bool IsConditionMet(out List<IFaction> factions)
    {
        factions = CrewmateFaction;
        winReason = WinReason.TasksComplete;


        bool hasAliveImpostor = false;
        bool hasOneTaskDoer = false;
        foreach (CustomRole role in Game.GetAllPlayers().Select(p => p.GetCustomRole()))
        {
            if (role.MyPlayer.Data.Tasks.Count > 0) hasOneTaskDoer = true;
            if (role.MyPlayer.IsAlive() && role.Faction is not Crewmates && role.RealRole.IsImpostor()) hasAliveImpostor = true;
            if (hasOneTaskDoer && hasAliveImpostor) break;
        }

        if (hasAliveImpostor && hasOneTaskDoer) return GameManager.Instance.CheckTaskCompletion();

        winReason = WinReason.FactionLastStanding;
        return true;
    }

    public WinReason GetWinReason() => winReason;

    public int Priority() => -1;
}