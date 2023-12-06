using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lotus.Addons;
using Lotus.API;
using Lotus.Roles2.Interfaces;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles2;

public class RoleMetadata: IEnumerable<KeyValuePair<NamespacedKey, object>>, IRoleComponent
{
    private readonly Dictionary<NamespacedKey, ConcatFunc> concats = new();
    private readonly Dictionary<NamespacedKey, object> values = new();

    public RoleMetadata(RoleMetadata roleMetadata) => values = new Dictionary<NamespacedKey, object>(roleMetadata.values);
    public RoleMetadata()
    {
        Set(TaskContainer.Key, TaskContainer.None);
        Set(RoleProperties.Key, new RoleProperties());
    }

    internal RoleMetadata Combine(IEnumerable<RoleMetadata> metadata)
    {
        metadata.SelectMany(md => md).ForEach(kvp => values[kvp.Key] = kvp.Value);

        concats.ForEach(concat =>
        {
            NamespacedKey key = concat.Key;
            if (concats.TryGetValue(key, out ConcatFunc? concatFunc))
            {
                if (values.TryGetValue(key, out object? value)) values[key] = concatFunc.Func(value);
                else concatFunc.Fallback.IfPresent(v => values[key] = v);
            }
        });

        return this;
    }

    public void Concatenating(NamespacedKey key, Func<object, object> concatFunc) => concats[key] = new ConcatFunc(concatFunc);
    public void Concatenating(NamespacedKey key, Func<object, object> concatFunc, object defaultValue) => concats[key] = new ConcatFunc(concatFunc, defaultValue);

    public T Get<T>(NamespacedKey<T> key) => (T?)values.GetValueOrDefault(key)!;
    public T GetOrDefault<T>(NamespacedKey<T> key, T fallback) => (T)(values.GetValueOrDefault(key, fallback!));
    public Optional<T> GetOrEmpty<T>(NamespacedKey<T> key) => values.GetOptional(key).Map(o => (T)o);

    public void Set<T>(NamespacedKey<T> key, T value) => values[key] = value!;

    public void Chain(LotusAddon addon, Action<MetadataAppender> appender) => Chain(addon.Name, appender);

    public void Chain(string addonName, Action<MetadataAppender> appender)
    {
        appender(new MetadataAppender(addonName, this));
    }

    public IRoleComponent Instantiate(SetupHelper setupHelper, PlayerControl player) => new RoleMetadata(this);

    private class ConcatFunc
    {
        public Func<object, object> Func { get; }
        public Optional<object> Fallback { get; }

        public ConcatFunc(Func<object, object> func, object fallback)
        {
            Func = func;
            Fallback = Optional<object>.NonNull(fallback);
        }

        public ConcatFunc(Func<object, object> func)
        {
            Func = func;
            Fallback = Optional<object>.Null();
        }
    }

    public IEnumerator<KeyValuePair<NamespacedKey, object>> GetEnumerator() => values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public class MetadataAppender
    {
        private string addonName;
        private RoleMetadata metadata;

        public MetadataAppender(string addonName, RoleMetadata metadata)
        {
            this.addonName = addonName;
            this.metadata = metadata;
        }

        public MetadataAppender Set<T>(string key, T value)
        {
            metadata.Set(new NamespacedKey<T>(addonName, key), value);
            return this;
        }
    }
}