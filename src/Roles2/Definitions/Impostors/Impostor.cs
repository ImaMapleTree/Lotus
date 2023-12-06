using Lotus.Roles2.Attributes.Options;

namespace Lotus.Roles2.Definitions.Impostors;

public class Impostor: ImpostorRoleDefinition
{
    public class Triggers<T>: RoleTriggers<T> where T: RoleDefinition
    {

    }

    public class KillCooldownAttribute: FloatRangeOptionAttribute
    {
        public KillCooldownAttribute(float min, float max, float step, int defaultIndex = 0) : base(min, max, step, defaultIndex, SuffixType.Seconds) { }
    }
}