using AmongUs.GameOptions;

namespace Lotus.Roles.RoleGroups.Vanilla;

public class Scientist: Crewmate
{
    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).VanillaRole(RoleTypes.Scientist);
}