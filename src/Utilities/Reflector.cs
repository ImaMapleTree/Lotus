using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lotus.Utilities;

// ReSharper disable once InconsistentNaming
public interface Reflector
{
    public Type RepresentedType { get; }
    public string Name { get; }

    public void SetValue(object instance, object? value);
    public object? GetValue(object instance);
    public TAttribute? GetAttribute<TAttribute>() where TAttribute : Attribute;
    public List<TAttribute> GetAttributes<TAttribute>() where TAttribute : Attribute;
}

public class PropertyReflector: Reflector
{
    public PropertyInfo Property { get; }
    public Type RepresentedType => Property.PropertyType;
    public string Name => Property.Name;

    public PropertyReflector(PropertyInfo property)
    {
        Property = property;
    }

    public void SetValue(object instance, object? value)
    {
        Property.SetValue(instance, value);
    }

    public object? GetValue(object instance) => Property.GetValue(instance);

    public TAttribute? GetAttribute<TAttribute>() where TAttribute : Attribute => Property.GetCustomAttribute<TAttribute>();
    public List<TAttribute> GetAttributes<TAttribute>() where TAttribute : Attribute => Property.GetCustomAttributes<TAttribute>().ToList();
}

public class FieldReflector : Reflector
{
    public FieldInfo Field { get; }
    public Type RepresentedType => Field.FieldType;
    public string Name => Field.Name;

    public FieldReflector(FieldInfo field)
    {
        Field = field;
    }

    public void SetValue(object instance, object? value)
    {
        Field.SetValue(instance, value);
    }

    public object? GetValue(object instance) => Field.GetValue(instance);

    public TAttribute? GetAttribute<TAttribute>() where TAttribute : Attribute => Field.GetCustomAttribute<TAttribute>();
    public List<TAttribute> GetAttributes<TAttribute>() where TAttribute : Attribute => Field.GetCustomAttributes<TAttribute>().ToList();
}

