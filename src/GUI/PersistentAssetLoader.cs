using System;
using System.Collections.Generic;
using TOHTOR.Utilities;
using UnityEngine;
using VentLib.Logging;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Harmony.Attributes;

namespace TOHTOR.GUI;

[RegisterInIl2Cpp]
internal class PersistentAssetLoader: MonoBehaviour
{
    private static Dictionary<string, SpriteRenderer> _spriteRenderers = new();
    private static readonly Dictionary<string, (string path, int pixelsPerUnit)> SpriteInfo = new();
    private static bool _initialized;

    private static PersistentAssetLoader _instance = null!;
    private List<GameObject> anchors = new();

    public PersistentAssetLoader(IntPtr intPtr) : base(intPtr)
    {
        if (!_initialized) LoadAssets();

        _initialized = true;
        _instance = this;
    }

    private void LoadAssets()
    {
        SpriteInfo.ForEach(si => LoadSprite(si.Key, si.Value.path, si.Value.pixelsPerUnit));
    }

    private void LoadSprite(string key, string path, int pixelsPerUnit)
    {
        VentLogger.Debug($"Loading Persistent Sprite: {key} => {path}", "PersistentAssetLoader");
        GameObject anchor = gameObject.CreateChild("Anchor");
        anchors.Add(anchor);
        SpriteRenderer render = anchor.AddComponent<SpriteRenderer>();
        render.sprite = Utils.LoadSprite(path, pixelsPerUnit);
        render.enabled = false;
        _spriteRenderers[key] = render;
    }

    public static Func<Sprite> RegisterSprite(string key, string path, int pixelsPerUnit)
    {
        SpriteInfo.Add(key, (path, pixelsPerUnit));
        if (_initialized) _instance.LoadSprite(key, path, pixelsPerUnit);
        return () => _spriteRenderers[key].sprite;
    }

    public static Sprite GetSprite(string key) => _spriteRenderers[key].sprite;

    [QuickPostfix(typeof(DiscordManager), nameof(DiscordManager.Start))]
    public static void HookToDiscordManager(MainMenuManager __instance)
    {
        __instance.gameObject.AddComponent<PersistentAssetLoader>();
    }
}