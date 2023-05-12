
using TOHTOR.Roles.Internals;

namespace TOHTOR.Roles.RoleGroups.Coven;

public class Coven: NotImplemented
{
    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier
            .SpecialType(SpecialType.Coven);
}