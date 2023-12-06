using System;

namespace Lotus.Roles2.Attributes.Roles;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class RoleActionOverrideAttribute: Attribute
{
    public Type TargetType;
    public string TargetMethod;


    public RoleActionOverrideAttribute(Type targetType, string targetMethod)
    {
        this.TargetType = targetType;
        this.TargetMethod = targetMethod;
    }
}