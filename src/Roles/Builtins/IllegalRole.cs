using System;

namespace Lotus.Roles.Builtins;

public sealed class IllegalRole: EmptyRole
{
    protected override void Setup(PlayerControl player) => throw new InvalidOperationException("This role is for internal use only and cannot be assigned");
}