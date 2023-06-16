using System.Linq;
using Lotus.Managers;
using Lotus.Roles;
using VentLib.Utilities.Extensions;

namespace Lotus.Gamemodes.Standard.Lotteries;

public class SubRoleLottery: RoleLottery
{
    public SubRoleLottery() : base(CustomRoleManager.Special.IllegalRole)
    {
        CustomRoleManager.AllRoles.Where(r => r.RoleFlags.HasFlag(RoleFlag.IsSubrole)).ForEach(r => AddRole(r));
    }
}