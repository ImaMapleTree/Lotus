using System.Linq;
using Lotus.Factions.Impostors;
using Lotus.Managers;
using Lotus.Roles;
using VentLib.Utilities.Extensions;

namespace Lotus.Gamemodes.Standard.Lotteries;

public class ImpostorLottery: RoleLottery
{
    public ImpostorLottery() : base(CustomRoleManager.Static.Impostor)
    {
        CustomRoleManager.AllRoles.Where(r => r.Faction is ImpostorFaction).ForEach(r => AddRole(r));
    }
}