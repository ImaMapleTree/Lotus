using System;

namespace TOHTOR.Roles;

[Flags]
public enum RoleFlag
{
    Hidden = 1,
    DontRegisterOptions = 2,
    Unassignable = 4,
    IncrementChanceByFives = 8,
    RemoveRolePercent = 16,
    RemoveRoleMaximum = 32,

    VariationRole = Hidden | Unassignable | IncrementChanceByFives | RemoveRoleMaximum | DontRegisterOptions,
    TransformationRole = Hidden | Unassignable | RemoveRolePercent | RemoveRoleMaximum | DontRegisterOptions
}