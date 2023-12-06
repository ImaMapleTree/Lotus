using System;
using HarmonyLib;

namespace Lotus.Roles2;

internal static class CloneUtils
{
    public static T Clone<T>(T obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        return (T)AccessTools.Method(typeof(Object), "MemberwiseClone").Invoke(obj, Array.Empty<object?>())!;
    }
}