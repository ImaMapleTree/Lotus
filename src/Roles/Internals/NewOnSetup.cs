using System.Reflection;

namespace Lotus.Roles.Internals;

public struct NewOnSetup
{
    public FieldInfo FieldInfo;
    public bool UseCloneIfPresent;

    public NewOnSetup(FieldInfo fieldInfo, bool useCloneIfPresent)
    {
        FieldInfo = fieldInfo;
        UseCloneIfPresent = useCloneIfPresent;
    }
}