using System.Reflection;
using Lotus.Roles.Internals.Attributes;

namespace Lotus.Roles.Internals;

public class ModifiedAction: RoleAction
{
    public ModifiedBehaviour Behaviour { get; }

    public ModifiedAction(ModifiedLotusActionAttribute attribute, MethodInfo method) : base(attribute, method)
    {
        Behaviour = attribute.Behaviour;
    }

    public override void Execute(object role, object[] args)
    {
        Method.InvokeAligned(((AbstractBaseRole)role).Editor!, args);
    }

    public override void ExecuteFixed(object role)
    {
        Method.Invoke(((AbstractBaseRole)role).Editor!, null);
    }
}