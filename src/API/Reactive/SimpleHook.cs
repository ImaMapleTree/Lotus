using System;
using System.Linq;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.API.Reactive;

public class SimpleHook<T> : OrderedDictionary<string, HookAction<T>>, Hook<T> where T: IHookEvent
{
    public bool Exists(string key) => base.ContainsKey(key);

    public bool TryGet(string key, out Action<T>? eventConsumer)
    {
        eventConsumer = null;
        if (!Exists(key)) return false;
        eventConsumer = base[key].Action;
        return true;
    }

    public bool TryGet(string key, out HookAction<T>? eventConsumer)
    {
        eventConsumer = null;
        if (!Exists(key)) return false;
        eventConsumer = base[key];
        return true;
    }

    public Hook<T> Bind(string key, Action<T> eventConsumer, bool replace = false, Priority priority = Priority.Normal)
    {
        HookAction<T> hookAction = new() { Action = eventConsumer, Priority = priority };
        if (replace) base[key] = hookAction;
        else base.Add(key, hookAction);
        return this;
    }

    public bool Unbind(string key)
    {
        if (!Exists(key)) return false;
        base.Remove(key);
        return true;
    }

    public void Propagate(T hookEvent)
    {
        base.GetValues().OrderBy(o => o.Priority).ForEach(ha => ha.Action.Invoke(hookEvent));
    }
}

public struct HookAction<T>
{
    public Action<T> Action;
    public Priority Priority;
}