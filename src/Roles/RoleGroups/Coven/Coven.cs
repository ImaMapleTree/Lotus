
using Lotus.Roles.Internals;

namespace Lotus.Roles.RoleGroups.Coven;

public class Coven: NotImplemented
{
    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier
            .SpecialType(Internals.SpecialType.Coven);
}