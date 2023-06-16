using System.Collections.Generic;
using AmongUs.GameOptions;
using Lotus.API.Stats;
using Lotus.Factions;
using Lotus.Roles.RoleGroups.Stock;

namespace Lotus.Roles.RoleGroups.Vanilla;

public class Crewmate : TaskRoleBase
{
    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.VanillaRole(RoleTypes.Crewmate).Faction(FactionInstances.Crewmates).RoleColor("#b6f0ff");

    public override List<Statistic> Statistics() => new() { VanillaStatistics.TasksComplete };
}

