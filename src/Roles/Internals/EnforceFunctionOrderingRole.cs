using System;

namespace Lotus.Roles.Internals;

/// <summary>
/// This role is used internally to enforce the order of certain calls when statically loading in the vanilla roles.
/// </summary>
internal sealed class EnforceFunctionOrderingRole: CustomRole
{
    private readonly Action action;
    public EnforceFunctionOrderingRole(Action action) => this.action = action;

    internal override void Solidify()
    {
        action();
        base.Solidify();
    }

    protected override void Setup(PlayerControl player) => throw new InvalidOperationException("This role is for internal use only and cannot be assigned");

    protected override RoleModifier Modify(RoleModifier roleModifier) => roleModifier.RoleFlags(RoleFlag.Hidden | RoleFlag.DontRegisterOptions | RoleFlag.Unassignable | RoleFlag.DoNotTranslate);
}