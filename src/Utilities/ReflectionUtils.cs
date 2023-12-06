using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Lotus.Utilities;

public class ReflectionUtils
{
    public static bool TrySet(PropertyInfo property, object instance, object? value)
    {
        MethodInfo? setMethod = property.GetSetMethod(true);
        Action<object, object?>? setAction = setMethod != null ? (o, v) => setMethod.Invoke(o, new[]{v}) : null;
        if (setAction == null)
        {
            FieldInfo? writeableField = property.DeclaringType!.GetRuntimeFields().FirstOrDefault(a => Regex.IsMatch( a.Name, $@"\A<{property.Name}>k__BackingField\Z" ));
            setAction = writeableField != null ? (o, v) => writeableField.SetValue(o, v) : null;
        }

        if (setAction == null) return false;
        setAction(instance, value);
        return true;
    }

    public static bool TrySet(FieldInfo field, object instance, object? value)
    {
        try
        {
            field.SetValue(instance, value);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}