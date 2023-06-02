using System.Reflection;
using Lotus.Roles.Internals.Attributes;
using VentLib.Logging;

namespace Lotus.Roles.Internals;

public class RoleAction
{
    public RoleActionType ActionType { get; }
    public bool TriggerWhenDead { get; }
    public Priority Priority { get; }
    public bool Blockable { get; }

    internal RoleActionAttribute Attribute;
    internal MethodInfo Method;
    internal object? Executer;

    public RoleAction(RoleActionAttribute attribute, MethodInfo method)
    {
        this.Method = method;
        this.TriggerWhenDead = attribute.WorksAfterDeath;
        this.ActionType = attribute.ActionType;
        this.Priority = attribute.Priority;
        this.Blockable = attribute.Blockable;
        this.Attribute = attribute;
    }

    public virtual void Execute(AbstractBaseRole role, object[] args)
    {
        VentLogger.Log(LogLevel.Trace, $"RoleAction(type={ActionType}, priority={Priority}, method={Method}, executer={Executer ?? role}))", "RoleAction::Execute");
        Method.InvokeAligned(Executer ?? role, args);
    }

    public virtual void ExecuteFixed(AbstractBaseRole role)
    {
        Method.Invoke(Executer ?? role, null);
    }

    public RoleAction Clone()
    {
        return (RoleAction)this.MemberwiseClone();
    }

    public override string ToString() => $"RoleAction(type={ActionType}, Priority={Priority}, Blockable={Blockable})";
}