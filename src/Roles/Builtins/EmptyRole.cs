using UnityEngine;

namespace Lotus.Roles.Builtins;

public class EmptyRole: CustomRole
{
    protected override RoleModifier Modify(RoleModifier roleModifier) => roleModifier
        .RoleColor(Color.grey)
        .RoleFlags(RoleFlag.Hidden | RoleFlag.Unassignable | RoleFlag.CannotWinAlone | RoleFlag.DoNotTranslate | RoleFlag.DontRegisterOptions);
}