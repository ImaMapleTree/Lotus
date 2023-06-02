using UnityEngine;

namespace Lotus.Utilities;

public class AssetLoader
{
    private const string AssetPath = "Lotus.assets";

    public static Sprite LoadSprite(string path, float pixelsPerUnit, bool linear = false, int mipMapLevels = 0)
    {
        if (path.StartsWith('.')) path = AssetPath + path;
        else path = AssetPath + "." + path;
        return Utils.LoadSprite(path, pixelsPerUnit, linear, mipMapLevels);
    }
}