using System.Collections.Generic;
using AmongUs.GameOptions;
using Lotus.API.Stats;
using Lotus.Factions;
using Lotus.Roles.Builtins.Base;
using Lotus.Roles.Internals.Enums;

namespace Lotus.Roles.Builtins.Vanilla;

public class Crewmate : TaskRoleBase
{
    internal override LotusRoleType LotusRoleType { get; set; } = LotusRoleType.Crewmates;

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.VanillaRole(RoleTypes.Crewmate).Faction(FactionInstances.Crewmates).RoleColor("#b6f0ff");

    public override List<Statistic> Statistics() => new() { VanillaStatistics.TasksComplete };
}

