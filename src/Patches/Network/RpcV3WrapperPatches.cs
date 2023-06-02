using Hazel;
using VentLib.Networking.RPC;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.Patches.Network;

[LoadStatic]
public class RpcV3WrapperPatches
{
    [QuickPrefix(typeof(PlayerControl), nameof(PlayerControl.RpcSetName))]
    public static bool RpcSetName(PlayerControl __instance, string name)
    {

        __instance.SetName(name);
        RpcV3 rpcV3 = (RpcV3)RpcV3.Immediate(__instance.NetId, RpcCalls.SetName, SendOption.None).Write(name);
        rpcV3.Send();

        return false;
    }
}