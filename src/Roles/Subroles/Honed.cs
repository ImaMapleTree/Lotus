using UnityEngine;

namespace Lotus.Roles.Subroles;

public class Honed: Subrole
{
    public override string Identifier() => "乂";

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.45f, 0.64f, 0.4f)).RoleFlags(RoleFlag.Hidden | RoleFlag.Unassignable | RoleFlag.DontRegisterOptions);
}