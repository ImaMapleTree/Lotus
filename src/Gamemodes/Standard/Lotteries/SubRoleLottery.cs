using Lotus.Managers;
using Lotus.Roles;

namespace Lotus.Gamemodes.Standard.Lotteries;

public class SubRoleLottery: RoleLottery
{
    public SubRoleLottery() : base(CustomRoleManager.Special.IllegalRole)
    {
        CustomRoleManager.ModifierRoles.ForEach(r => AddRole(r));
    }
}