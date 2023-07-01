using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.Patches.Network;

public class ServerAuthPatch
{
    [QuickPostfix(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
    public static void ConstantVersionPatch(ref int __result)
    {
        __result = Constants.GetVersion(2023, 1, 11, 0);
    }
}