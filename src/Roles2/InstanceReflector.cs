using System;
using System.Collections.Generic;
using Lotus.Utilities;

namespace Lotus.Roles2;

public abstract class InstanceReflector: Reflector
{
    public object Instance { get; }
    public abstract Type RepresentedType { get; }
    public abstract string Name { get; }

    protected InstanceReflector(object instance)
    {
        this.Instance = instance;
    }

    public abstract void SetValue(object instance, object? value);

    public abstract object? GetValue(object instance);

    public abstract TAttribute? GetAttribute<TAttribute>() where TAttribute : Attribute;
    public abstract List<TAttribute> GetAttributes<TAttribute>() where TAttribute : Attribute;

    public void SetValue(object? value) => SetValue(Instance, value);

    public object? GetValue() => GetValue(Instance);
}