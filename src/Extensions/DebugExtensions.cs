using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2CppSystem.ComponentModel;
using Lotus.Logging;
using UnityEngine;
using VentLib.Logging;
using VentLib.Utilities.Extensions;
using Component = UnityEngine.Component;
using Object = UnityEngine.Object;

namespace Lotus.Extensions;

public static class DebugExtensions
{
    public static void DebugLog(this object obj, string prefixText = "", string tag = "DebugLog", ConsoleColor color = ConsoleColor.DarkGray)
    {
        LogLevel tempLevel = new("OBJ", 0, color);
        VentLogger.Log(tempLevel,$"{prefixText}{obj}", tag);
    }

    public static void Debug(this IEnumerable<Component> components)
    {
        DevLogger.Log(components.Select(c => (c.TypeName(), c.name)).Join());
    }
    
    public static void Debug(this IEnumerable<Object> components)
    {
        DevLogger.Log(components.Select(c => (c.TypeName(), c.name)).Join());
    }

    public static void Debug(this MonoBehaviour monoBehaviour, bool noChildren = false, bool includeInactive = false)
    {
        if (noChildren) monoBehaviour.GetComponents<Component>().Debug();
        else monoBehaviour.GetComponentsInChildren<Component>(includeInactive).Debug();
    }

    public static void Debug(this GameObject gameObject, bool noChildren = false,  bool includeInactive = false)
    {
        if (noChildren) gameObject.GetComponents<Component>().Debug();
        else gameObject.GetComponentsInChildren<Component>(includeInactive).Debug();
    }
}