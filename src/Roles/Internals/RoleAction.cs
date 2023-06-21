using System.Reflection;
using Lotus.API;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using VentLib.Logging;
using VentLib.Utilities.Debug.Profiling;

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
        VentLogger.Trace($"RoleAction(type={ActionType}, executer={Executer ?? role}, priority={Priority}, method={Method}))", "RoleAction::Execute");
        Profiler.Sample sample1 = Profilers.Global.Sampler.Sampled($"Action::{ActionType}");
        Profiler.Sample sample2 = Profilers.Global.Sampler.Sampled((Method.ReflectedType?.FullName ?? "") + "." + Method.Name);
        Method.InvokeAligned(Executer ?? role, args);
        sample1.Stop();
        sample2.Stop();
    }

    public virtual void ExecuteFixed(AbstractBaseRole role)
    {
        Method.Invoke(Executer ?? role, null);
    }

    public RoleAction Clone()
    {
        return (RoleAction)this.MemberwiseClone();
    }

    public override string ToString() => $"RoleAction(type={ActionType}, executer={Executer}, priority={Priority}, method={Method}))";
}