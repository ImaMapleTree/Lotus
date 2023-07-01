namespace Lotus.Roles.Internals.Attributes;

/*/// <summary>
/// Marks any role editor class for automatically linking at the start of the runtime.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class StaticEditor : Attribute
{
    internal static void Register(Assembly assembly)
    {
        assembly.GetTypes()
            .Where(type => type.GetCustomAttribute<StaticEditor>() != null)
            .Do(CustomRoleManager.LinkEditor);
    }
}*/