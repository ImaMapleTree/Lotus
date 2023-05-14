using VentLib.Logging;

namespace Lotus.Roles.RoleGroups.Coven;

public class Conjuror: CustomRole
{
    protected override RoleModifier Modify(RoleModifier roleModifier)
    {
        VentLogger.Warn($"{this.RoleName} Not Implemented Yet", "RoleImplementation");
        return roleModifier;
    }
}