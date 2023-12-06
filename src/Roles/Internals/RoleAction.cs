using System.Reflection;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.Logging;
using Lotus.Roles.Internals.Attributes;

namespace Lotus.Roles.Internals;

public class RoleAction: LotusAction
{
    public ActionFlag Flags { get; }
    public bool TriggerWhenDead { get; }
    public bool Blockable;

    public RoleAction(RoleActionAttribute attribute, MethodInfo method) : base(attribute, method)
    {
        this.Method = method;
        this.TriggerWhenDead = attribute.WorksAfterDeath;
        this.Blockable = attribute.Blockable;
        this.Flags = attribute.ActionFlags;
    }

    public RoleAction(RoleActionAttribute attribute, MethodInfo method, object executer): base(attribute, method)
    {
        DevLogger.Log($"Registered new action wth executer: {executer}");
        this.Executer = executer;
        this.TriggerWhenDead = attribute.WorksAfterDeath;
        this.Blockable = attribute.Blockable;
        this.Flags = attribute.ActionFlags;
    }

    public bool CanExecute(PlayerControl executer, PlayerControl? source)
    {
        if (!executer.IsAlive() && !TriggerWhenDead) return false;
        if (!ReferenceEquals(source, null) && !Flags.HasFlag(ActionFlag.GlobalDetector) && executer.PlayerId != source.PlayerId) return false;
        return true;
    }

    public new RoleAction Clone()
    {
        return (RoleAction)this.MemberwiseClone();
    }

    public override string ToString() => $"RoleAction(type={ActionType}, executer={Executer}, priority={Priority}, method={Method}))";
}