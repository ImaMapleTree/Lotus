using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Lotus.Utilities;

public class AssetRegistry
{
    public AssetLoader Loader { get; } = new();
    private Dictionary<string, Func<LazySprite>> assetBindings = new();

    public AssetBinding CreateEntry(string assetKey, Assembly? assembly = null)
    {
        return new AssetBinding(assetKey, this, assembly ?? Assembly.GetCallingAssembly());
    }

    public AssetRegistry CreateEntry(string resourcePath, float pixelsPerUnit, bool linear = false, int mipMapLevels = 0, Assembly? assembly = null)
    {
        assetBindings[resourcePath] = () => Loader.LoadLazy(resourcePath, pixelsPerUnit, linear, mipMapLevels, assembly ?? Assembly.GetCallingAssembly());
        return this;
    }

    public bool HasEntry(string assetKey) => assetBindings.ContainsKey(assetKey);

    public Sprite GetSprite(string assetKey)
    {
        return assetBindings[assetKey].Invoke().Get();
    }

    public LazySprite GetLazySprite(string assetKey)
    {
        return assetBindings[assetKey].Invoke();
    }

    public class AssetBinding
    {
        private readonly string key;
        private readonly AssetRegistry registry;
        private readonly Assembly assembly;

        public AssetBinding(string key, AssetRegistry registry, Assembly assembly)
        {
            this.key = key;
            this.registry = registry;
            this.assembly = assembly;
        }

        public AssetRegistry Sprite(string resourcePath, float pixelsPerUnit, bool linear = false, int mipMapLevels = 0, Assembly? assembly = null)
        {
            registry.assetBindings[key] = () => registry.Loader.LoadLazy(resourcePath, pixelsPerUnit, linear, mipMapLevels, assembly ?? this.assembly);
            return registry;
        }
    }
}