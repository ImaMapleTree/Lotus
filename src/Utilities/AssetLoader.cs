using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using VentLib.Utilities.Extensions;

namespace Lotus.Utilities;

public class AssetLoader
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(AssetLoader));

    private const string AssetPath = "Lotus.assets";
    private readonly Dictionary<string, LazySprite> cachedLazySprites = new();

    public static Sprite LoadSprite(string path, float pixelsPerUnit = 100f, bool linear = false, int mipMapLevel = 0, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        Sprite sprite;
        MemoryStream memoryStream = new();
        try
        {
            Stream? stream = assembly.GetManifestResourceStream(path);
            if (stream == null) throw new NullReferenceException("Resource stream was null.");
            Texture2D texture = new(1, 1, TextureFormat.ARGB32, true, linear);
            stream.CopyTo(memoryStream);
            ImageConversion.LoadImage(texture, memoryStream.ToArray());
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.texture.requestedMipmapLevel = mipMapLevel;
        }
        catch (Exception)
        {
            log.Exception($"Error Loading Asset: \"{path}\"", "LoadImage");
            throw;
        }
        finally
        {
            memoryStream.Close();
        }

        return sprite;
    }

    internal static Sprite LoadLotusSprite(string path, float pixelsPerUnit, bool linear = false, int mipMapLevels = 0)
    {
        if (path.StartsWith('.')) path = AssetPath + path;
        else path = AssetPath + "." + path;
        return LoadSprite(path, pixelsPerUnit, linear, mipMapLevels);
    }

    public LazySprite LoadLazy(string resourcePath, float pixelsPerUnit, bool linear = false, int mipMapLevels = 0, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        return cachedLazySprites.GetOrCompute(resourcePath, () => new LazySprite(() => LoadSprite(resourcePath, pixelsPerUnit, linear, mipMapLevels, assembly)));
    }

    internal LazySprite LotusLoadLazy(string resourcePath, float pixelsPerUnit, bool linear = false, int mipMapLevels = 0, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        if (resourcePath.StartsWith('.')) resourcePath = AssetPath + resourcePath;
        else resourcePath = AssetPath + "." + resourcePath;
        return LoadLazy(resourcePath, pixelsPerUnit, linear, mipMapLevels, assembly);
    }


}