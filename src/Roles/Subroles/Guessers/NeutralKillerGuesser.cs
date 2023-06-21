using Lotus.Extensions;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.RoleGroups.Stock;

namespace Lotus.Roles.Subroles.Guessers;

public class NeutralKillerGuesser: Guesser
{
    public override bool IsAssignableTo(PlayerControl player)
    {
        CustomRole role = player.GetCustomRole();
        return role is not GuesserRoleBase && role.SpecialType is SpecialType.NeutralKilling;
    }
}