using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Lotus.Logging;
using Lotus.Roles.Internals.Interfaces;
using Lotus.Roles2.Attributes;
using Lotus.Roles2.Interfaces;
using Lotus.Utilities;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles2;

public sealed class SetupHelper
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(SetupHelper));

    internal Dictionary<FieldInfo, object?> Fields { get; } = new();
    internal Dictionary<PropertyInfo, object?> Properties { get; } = new();
    internal List<InstanceReflector> Reflectors { get; } = new();

    internal static SetupHelper Reflect(object obj)
    {
        SetupHelper helper = new();
        helper.GenerateReflectionInformation(obj);
        return helper;
    }

    public T Clone<T>(T obj)
    {
        T result = CloneUtils.Clone(obj);
        GenerateReflectionInformation(result!);
        return result;
    }

    internal void GenerateReflectionInformation(object obj)
    {
        GenerateFieldInformation(obj);
        GeneratePropertyInformation(obj);
    }

    private void GenerateFieldInformation(object obj)
    {
        Type objectType = obj.GetType();
        SetupInjected? setupInjected = objectType.GetCustomAttribute<SetupInjected>();
        objectType.GetFields(AccessFlags.InstanceAccessFlags).ForEach(field => ReflectFields(obj, field, setupInjected));
    }

    private void GeneratePropertyInformation(object obj)
    {
        Type objectType = obj.GetType();
        SetupInjected? setupInjected = objectType.GetCustomAttribute<SetupInjected>();
        objectType.GetProperties(AccessFlags.InstanceAccessFlags | BindingFlags.FlattenHierarchy).ForEach(property => ReflectProperties(obj, property, setupInjected));
    }

    private void ReflectFields(object obj, FieldInfo field, SetupInjected? setupInjected)
    {
        setupInjected = field.GetCustomAttribute<SetupInjected>() ?? setupInjected;
        if (field.GetCustomAttribute<SetupInjected.Excluded>() != null) setupInjected = null;

        object? currentValue = field.GetValue(obj);
        Fields[field] = currentValue;
        InstanceReflector reflector = new DictionaryAwareFieldReflector(obj, field, Fields);
        Reflectors.Add(reflector);

        if (!ReturnInjectedValue(currentValue, field.FieldType, setupInjected, out object? newValue)) return;
        if (ReflectionUtils.TrySet(field, obj, newValue)) reflector.SetValue(newValue);
    }

    private void ReflectProperties(object obj, PropertyInfo property, SetupInjected? setupInjected)
    {
        setupInjected = property.GetCustomAttribute<SetupInjected>() ?? setupInjected;
        if (property.GetCustomAttribute<SetupInjected.Excluded>() != null) setupInjected = null;

        object? currentValue = null;
        try { currentValue = property.GetValue(obj); }
        catch { /* ignored */ }
        Properties[property] = currentValue;
        InstanceReflector reflector = new DictionaryAwarePropertyReflector(obj, property, Properties);
        Reflectors.Add(reflector);

        if (!ReturnInjectedValue(currentValue, property.PropertyType, setupInjected, out object? newValue)) return;
        if (ReflectionUtils.TrySet(property, obj, newValue)) reflector.SetValue(newValue);
    }

    private bool ReturnInjectedValue(object? originalValue, Type originalType, SetupInjected? setupInjected, out object? injectedValue)
    {
        injectedValue = null;

        InstantiateOnSetupAttribute? instantiateOnSetupAttribute = originalType.GetCustomAttribute<InstantiateOnSetupAttribute>();

        if (setupInjected == null)
        {
            if (instantiateOnSetupAttribute is not { IgnoreInjectionRules: true }) return false;
            if (originalValue != null && !instantiateOnSetupAttribute.IgnorePreSetValues) return false;
            injectedValue = AccessTools.CreateInstance(originalType);
            return true;
        }

        if (setupInjected.UseCloneIfPresent && originalValue is ICloneOnSetup cos)
        {
            injectedValue = cos.CloneIndiscriminate();
            return true;
        }

        MethodInfo? cloneMethod = originalType.GetMethod("Clone", AccessFlags.InstanceAccessFlags, Array.Empty<Type>());
        if (originalValue == null || cloneMethod == null || !setupInjected.UseCloneIfPresent)
            try {
                injectedValue = AccessTools.CreateInstance(originalType);
                return true;
            } catch (Exception e) {
                log.Exception(e);
                throw new ArgumentException($"Error during setup field-injection. Could not create instance with no-args constructor for type {originalType})");
            }

        try {
            injectedValue = cloneMethod.Invoke(originalValue, null)!;
            return true;
        }
        catch (Exception e) {
            log.Exception(e);
            throw new ArgumentException($"Error during setup field-injection. Could not clone original instance for type {originalType})");
        }
    }

    private class DictionaryAwarePropertyReflector: InstanceReflector
    {
        public override Type RepresentedType => property.PropertyType;
        public override string Name => property.Name;
        private Dictionary<PropertyInfo, object?> properties;
        private PropertyInfo property;

        public DictionaryAwarePropertyReflector(object instance, PropertyInfo property, Dictionary<PropertyInfo, object?> properties) : base(instance)
        {
            this.properties = properties;
            this.property = property;
        }

        public override void SetValue(object instance, object? value)
        {
            if (ReflectionUtils.TrySet(property, instance, value))
                properties[property] = value;
        }

        public override object? GetValue(object instance) => properties[property];

        public override TAttribute GetAttribute<TAttribute>() => property.GetCustomAttribute<TAttribute>()!;
        public override List<TAttribute> GetAttributes<TAttribute>() => property.GetCustomAttributes<TAttribute>().ToList();
    }

    private class DictionaryAwareFieldReflector : InstanceReflector
    {
        public override Type RepresentedType => field.FieldType;
        public override string Name => field.Name;
        private Dictionary<FieldInfo, object?> fields;
        private FieldInfo field;

        public DictionaryAwareFieldReflector(object instance, FieldInfo field, Dictionary<FieldInfo, object?> fields) : base(instance)
        {
            this.field = field;
            this.fields = fields;
        }

        public override void SetValue(object instance, object? value)
        {
            field.SetValue(instance, value);
            fields[field] = value;
        }

        public override object? GetValue(object instance) => fields[field];

        public override TAttribute GetAttribute<TAttribute>() => field.GetCustomAttribute<TAttribute>()!;
        public override List<TAttribute> GetAttributes<TAttribute>() => field.GetCustomAttributes<TAttribute>().ToList();
    }
}