using System.Linq;
using TOHTOR.Managers;
using TOHTOR.Roles;
using TOHTOR.Roles.Internals;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Gamemodes.Standard.Lotteries;

public class NeutralLottery: RoleLottery
{
    public NeutralLottery() : base(CustomRoleManager.Special.IllegalRole)
    {
        CustomRoleManager.AllRoles.Where(r => r.SpecialType is SpecialType.Neutral).ForEach(r => AddRole(r));
    }
}