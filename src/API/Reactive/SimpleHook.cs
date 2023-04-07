using System;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace TOHTOR.API.Reactive;

public class SimpleHook<T> : OrderedDictionary<string, Action<T>>, Hook<T> where T: IHookEvent
{
    public bool Exists(string key) => base.ContainsKey(key);

    public bool TryGet(string key, out Action<T>? eventConsumer)
    {
        eventConsumer = null;
        if (!Exists(key)) return false;
        eventConsumer = base[key];
        return true;
    }

    public void Bind(string key, Action<T> eventConsumer, bool replace = false)
    {
        if (replace) base[key] = eventConsumer;
        else base.Add(key ,eventConsumer);
    }

    public bool Unbind(string key)
    {
        if (!Exists(key)) return false;
        base.Remove(key);
        return true;
    }

    public void Propagate(T hookEvent)
    {
        base.GetValues().ForEach(v => v.Invoke(hookEvent));
    }
}