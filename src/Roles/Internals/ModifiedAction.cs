using System.Reflection;
using Lotus.Roles.Internals.Attributes;

namespace Lotus.Roles.Internals;

public class ModifiedAction: RoleAction
{
    public ModifiedBehaviour Behaviour { get; }

    public ModifiedAction(ModifiedActionAttribute attribute, MethodInfo method) : base(attribute, method)
    {
        Behaviour = attribute.Behaviour;
    }

    public override void Execute(AbstractBaseRole role, object[] args)
    {
        Method.InvokeAligned(role.Editor!, args);
    }

    public override void ExecuteFixed(AbstractBaseRole role)
    {
        Method.Invoke(role.Editor!, null);
    }
}