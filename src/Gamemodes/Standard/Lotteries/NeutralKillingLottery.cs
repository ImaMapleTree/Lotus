using System.Linq;
using Lotus.Managers;
using Lotus.Roles;
using Lotus.Roles.Internals;
using VentLib.Utilities.Extensions;

namespace Lotus.Gamemodes.Standard.Lotteries;

public class NeutralKillingLottery: RoleLottery
{
    // TODO: maybe change this default role
    public NeutralKillingLottery() : base(CustomRoleManager.Special.IllegalRole)
    {
        CustomRoleManager.AllRoles.Where(r => r.SpecialType is SpecialType.NeutralKilling or SpecialType.Undead).ForEach(r => AddRole(r));
    }
}