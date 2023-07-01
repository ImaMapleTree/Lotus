using System.Reflection;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using VentLib.Logging;
using VentLib.Utilities.Debug.Profiling;

namespace Lotus.API.Reactive.Actions;

public class LotusAction
{
    public LotusActionType ActionType { get; }
    public Priority Priority { get; }

    internal LotusActionAttribute Attribute;
    internal MethodInfo Method;
    internal object? Executer;

    public LotusAction(LotusActionAttribute attribute, MethodInfo method)
    {
        this.Method = method;
        this.ActionType = attribute.ActionType;
        this.Priority = attribute.Priority;
        this.Attribute = attribute;
    }

    public virtual void Execute(object role, object[] args)
    {
        VentLogger.Trace($"RoleAction(type={ActionType}, executer={Executer ?? role}, priority={Priority}, method={Method}))", "RoleAction::Execute");
        Profiler.Sample sample1 = Profilers.Global.Sampler.Sampled($"Action::{ActionType}");
        Profiler.Sample sample2 = Profilers.Global.Sampler.Sampled((Method.ReflectedType?.FullName ?? "") + "." + Method.Name);
        Method.InvokeAligned(Executer ?? role, args);
        sample1.Stop();
        sample2.Stop();
    }

    public virtual void ExecuteFixed(object role)
    {
        Method.Invoke(Executer ?? role, null);
    }

    public void SetExecuter(object executer)
    {
        Executer = executer;
    }

    public LotusAction Clone()
    {
        return (LotusAction)this.MemberwiseClone();
    }

    public override string ToString() => $"LotusAction(type={ActionType}, executer={Executer}, priority={Priority}, method={Method}))";

}