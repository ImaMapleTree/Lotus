using System.Linq;
using TOHTOR.Factions.Impostors;
using TOHTOR.Managers;
using TOHTOR.Roles;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Gamemodes.Standard.Lotteries;

public class ImpostorLottery: RoleLottery
{
    public ImpostorLottery() : base(CustomRoleManager.Static.Impostor)
    {
        CustomRoleManager.AllRoles.Where(r => r.Faction is ImpostorFaction).ForEach(r => AddRole(r));
    }
}