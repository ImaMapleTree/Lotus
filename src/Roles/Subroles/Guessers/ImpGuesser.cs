using Lotus.Extensions;
using Lotus.Factions.Impostors;
using Lotus.Roles.RoleGroups.Stock;

namespace Lotus.Roles.Subroles.Guessers;

public class ImpGuesser: Guesser
{
    public override bool IsAssignableTo(PlayerControl player)
    {
        CustomRole role = player.GetCustomRole();
        return role is not GuesserRoleBase && role.Faction is ImpostorFaction;
    }
}