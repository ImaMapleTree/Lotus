using AmongUs.GameOptions;

namespace Lotus.Roles.Builtins.Vanilla;

public class GuardianAngel: CustomRole
{
    public virtual bool CanBeKilled() => false;

    protected override RoleModifier Modify(RoleModifier roleModifier) => roleModifier.VanillaRole(RoleTypes.GuardianAngel);
}