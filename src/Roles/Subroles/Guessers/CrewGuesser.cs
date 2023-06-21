using Lotus.Extensions;
using Lotus.Factions.Crew;
using Lotus.Roles.RoleGroups.Stock;

namespace Lotus.Roles.Subroles.Guessers;

public class CrewGuesser: Guesser
{
    public override bool IsAssignableTo(PlayerControl player)
    {
        CustomRole role = player.GetCustomRole();
        return role is not GuesserRoleBase && role.Faction is Crewmates;
    }
}