using VentLib.Logging;

namespace TOHTOR.Roles.Subroles;

public class LastImpostor : Subrole
{
    public override string? Identifier() => "★";

    protected override RoleModifier Modify(RoleModifier roleModifier)
    {
        base.Modify(roleModifier);
        VentLogger.Warn($"{this.RoleName} Not Implemented Yet", "RoleImplementation");
        return roleModifier;
    }
}