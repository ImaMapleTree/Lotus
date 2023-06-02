using System.Linq;
using Lotus.Managers;
using Lotus.Roles;
using Lotus.Roles.Internals;
using VentLib.Utilities.Extensions;

namespace Lotus.Gamemodes.Standard.Lotteries;

public class NeutralLottery: RoleLottery
{
    public NeutralLottery() : base(CustomRoleManager.Special.IllegalRole)
    {
        CustomRoleManager.AllRoles.Where(r => r.SpecialType is SpecialType.Neutral).ForEach(r => AddRole(r));
    }
}