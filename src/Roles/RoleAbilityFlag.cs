using System;

namespace Lotus.Roles;

[Flags]
public enum RoleAbilityFlag
{
    CannotVent = 1,
    CannotSabotage = 2
}