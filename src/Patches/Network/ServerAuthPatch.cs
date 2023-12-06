using System.Collections.Generic;
using Lotus.Server;
using Lotus.Server.Interfaces;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.Patches.Network;

public class ServerAuthPatch
{
    public static bool IsLocal;
    private static Queue<byte> _ignoreBroadcastQueue = new();


    [QuickPostfix(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
    public static void ConstantVersionPatch(ref int __result)
    {
        // ReSharper disable once AssignmentInConditionalExpression
        if (IsLocal = _ignoreBroadcastQueue.TryDequeue(out _)) return;
        __result += 25;
    }


    [QuickPostfix(typeof(HostLocalGameButton), nameof(HostLocalGameButton.OnClick))]
    public static void OverrideLocalVersion(HostLocalGameButton __instance)
    {
        _ignoreBroadcastQueue.Enqueue(0);
    }
}