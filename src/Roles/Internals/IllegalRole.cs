using System;

namespace TOHTOR.Roles.Internals;

public sealed class IllegalRole: CustomRole
{
    protected override void Setup(PlayerControl player) => throw new InvalidOperationException("This role is for internal use only and cannot be assigned");

    protected override RoleModifier Modify(RoleModifier roleModifier) => roleModifier.RoleFlags(RoleFlag.Hidden | RoleFlag.DontRegisterOptions | RoleFlag.Unassignable);
}