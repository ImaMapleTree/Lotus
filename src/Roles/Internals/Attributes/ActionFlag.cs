using System;

namespace Lotus.Roles.Internals.Attributes;

[Flags]
public enum ActionFlag
{
    None = 0,
    GlobalDetector = 1,
    Unblockable = 2,
    WorksAfterDeath = 4
}