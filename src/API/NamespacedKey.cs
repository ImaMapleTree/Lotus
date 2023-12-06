using System;
using Lotus.Addons;
using Lotus.Extensions;

namespace Lotus.API;

public sealed class NamespacedKey<T>: NamespacedKey
{
    public string Namespace { get; }
    public string Key { get; }
    public Type Type { get; } = typeof(T);

    public NamespacedKey(string addonName, string key)
    {
        this.Namespace = addonName;
        this.Key = key;
    }

    public NamespacedKey(LotusAddon addon, string key)
    {
        this.Namespace = addon.Name;
        this.Key = key;
    }

    public override int GetHashCode() => HashCode.Combine(Namespace.SemiConsistentHash(), Key.SemiConsistentHash(), Type.SemiConsistentHash());
    public override bool Equals(object? obj) => obj is NamespacedKey namespacedKey && namespacedKey.Namespace == Namespace && namespacedKey.Key == Key && namespacedKey.Type == Type;
    public override string ToString() => $"{Namespace}::{Key}~{Type}";
}

// ReSharper disable once InconsistentNaming
public interface NamespacedKey
{
    public string Namespace { get; }
    public string Key { get; }
    public Type Type { get; }

    public static NamespacedKey<T> Lotus<T>(string key) => new("Lotus", key);
    public static NamespacedKey<T> LotusTrigger<T>(string key) => new("LotusTrigger", key);
}