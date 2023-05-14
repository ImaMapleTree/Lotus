using System.Linq;
using Lotus.Factions.Crew;
using Lotus.Managers;
using Lotus.Roles;
using VentLib.Utilities.Extensions;

namespace Lotus.Gamemodes.Standard.Lotteries;

public class CrewmateLottery: RoleLottery
{
    public CrewmateLottery() : base(CustomRoleManager.Static.Crewmate)
    {
        CustomRoleManager.AllRoles.Where(r => r is { Faction: Crewmates, IsSubrole: false }).ForEach(r => AddRole(r));
    }
}