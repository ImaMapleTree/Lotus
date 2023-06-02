using AmongUs.GameOptions;
using Hazel;
using Lotus.Extensions;
using Lotus.Logging;
using VentLib.Logging;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.RPC;

public static class CheckedRpc
{
    public static void CRpcShapeshift(this PlayerControl player, PlayerControl target, bool animate)
    {
        if (!player.IsAlive()) return;
        if (AmongUsClient.Instance.AmClient) player.Shapeshift(target, animate);

        RpcV3.Mass(SendOption.Reliable)
            .Start(player.NetId, RpcCalls.Shapeshift).Write(target).Write(animate).End()
            .SendExcluding(PlayerControl.LocalPlayer.GetClientId());
    }

    public static void CRpcRevertShapeshift(this PlayerControl player, bool animate)
    {
        VentLogger.Trace("CRevertShapeshift");
        if (!player.IsAlive()) return;
        if (AmongUsClient.Instance.AmClient) player.Shapeshift(player, animate);
        player.SetName(player.name);
        RpcV3.Mass(SendOption.Reliable)
            .Start(player.NetId, RpcCalls.Shapeshift).Write(player).Write(animate).End()
            .SendExcluding(PlayerControl.LocalPlayer.GetClientId());
    }

    public static void CRpcSetRole(this PlayerControl player, RoleTypes role)
    {
        if (player.IsHost()) player.SetRole(role);
        player.RpcSetRole(role);
    }
}