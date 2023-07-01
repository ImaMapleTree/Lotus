using System.Reflection;
using Lotus.API;
using Lotus.API.Reactive.Actions;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using VentLib.Logging;
using VentLib.Utilities.Debug.Profiling;

namespace Lotus.Roles.Internals;

public class RoleAction: LotusAction
{
    public bool TriggerWhenDead { get; }
    public bool Blockable;

    public RoleAction(RoleActionAttribute attribute, MethodInfo method) : base(attribute, method)
    {
        this.Method = method;
        this.TriggerWhenDead = attribute.WorksAfterDeath;
        this.Blockable = attribute.Blockable;
    }

    public new RoleAction Clone()
    {
        return (RoleAction)this.MemberwiseClone();
    }

    public override string ToString() => $"RoleAction(type={ActionType}, executer={Executer}, priority={Priority}, method={Method}))";
}