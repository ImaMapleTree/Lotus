using System;
using System.Reflection;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;

namespace Lotus.Roles2;

public class RoleActionStub
{
    public  RoleActionAttribute Attribute { get; }
    public Type RequiredExecuter;
    public readonly MethodInfo Method;

    public RoleActionStub(RoleActionAttribute attribute, MethodInfo method, Type requiredExecuter)
    {
        this.Attribute = attribute;
        this.Method = method;
        RequiredExecuter = requiredExecuter;
    }

    public RoleAction CreateAction(object executer) => new(Attribute, Method, executer);
}