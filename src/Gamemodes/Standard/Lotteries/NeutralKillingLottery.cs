using System.Linq;
using TOHTOR.Managers;
using TOHTOR.Roles;
using TOHTOR.Roles.Internals;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Gamemodes.Standard.Lotteries;

public class NeutralKillingLottery: RoleLottery
{
    // TODO: maybe change this default role
    public NeutralKillingLottery() : base(CustomRoleManager.Special.IllegalRole)
    {
        CustomRoleManager.AllRoles.Where(r => r.SpecialType is SpecialType.NeutralKilling).ForEach(r => AddRole(r));
    }
}