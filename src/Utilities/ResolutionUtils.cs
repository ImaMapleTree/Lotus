using UnityEngine;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Utilities;

public class ResolutionUtils
{
    public static bool IsFullscreen = Screen.fullScreen;

    public static (int width, int height)[] Resolutions = {
        (640, 360), (854, 480), (800, 600), (960, 540), (1024, 576), (1024, 768), (1152, 864), (1176, 664),
        (1280, 720), (1280, 800), (1280, 960), (1360, 768), (1366, 768), (1400, 1050), (1440, 900), (1600, 900),
        (1600, 1024), (1600, 1200), (1680, 1050), (1920, 1080), (1920, 1200), (1920, 1440), (2048, 1152), (2560, 1440),
        (5120, 2880)
    };

    public static (int width, int height)[] ResolutionsSixteenNine = {
        (640, 360), (854, 480), (960, 540), (1024, 576), (1280, 720), (1366, 768), (1600, 900), (1920, 1080), (2560, 1440), (5120, 2880)
    };

    public static int ResolutionIndex
    {
        get => Mathf.Clamp(_resolutionIndex != -1
            ? _resolutionIndex
            : ResolutionsSixteenNine.IndexOf(item => item.width == Screen.width && item.height == Screen.height), 0, ResolutionsSixteenNine.Length - 1);
        set => _resolutionIndex = Mathf.Clamp(value, 0, ResolutionsSixteenNine.Length - 1);
    }
    private static int _resolutionIndex = -1;

    public static void SetResolution(int width, int height, bool fullscreen = false) => ResolutionManager.SetResolution(width, height, fullscreen);
}