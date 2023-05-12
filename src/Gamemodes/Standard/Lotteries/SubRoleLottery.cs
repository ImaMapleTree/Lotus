using TOHTOR.Managers;
using TOHTOR.Roles;

namespace TOHTOR.Gamemodes.Standard.Lotteries;

public class SubRoleLottery: RoleLottery
{
    public SubRoleLottery() : base(CustomRoleManager.Special.IllegalRole)
    {
        CustomRoleManager.ModifierRoles.ForEach(r => AddRole(r));
    }
}