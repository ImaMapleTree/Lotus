using AmongUs.GameOptions;
using Hazel;
using TOHTOR.Extensions;
using VentLib.Logging;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace TOHTOR.RPC;

public static class CheckedRpc
{
    public static void CRpcShapeshift(this PlayerControl player, PlayerControl target, bool animate)
    {
        if (!player.IsAlive()) return;
        if (AmongUsClient.Instance.AmClient) player.Shapeshift(target, animate);
        RpcV3.Immediate(player.NetId, RpcCalls.Shapeshift, SendOption.None).Write(target).Write(animate).SendExcluding(PlayerControl.LocalPlayer.GetClientId());
    }

    public static void CRpcRevertShapeshift(this PlayerControl player, bool animate)
    {
        VentLogger.Trace("CRevertShapeshift");
        if (!player.IsAlive()) return;
        if (AmongUsClient.Instance.AmClient) player.Shapeshift(player, animate);
        player.SetName(player.name);
        RpcV3.Immediate(player.NetId, RpcCalls.Shapeshift, SendOption.None).Write(player).Write(animate).SendExcluding(PlayerControl.LocalPlayer.GetClientId());
    }

    public static void CRpcSetRole(this PlayerControl player, RoleTypes role)
    {
        if (player.IsHost()) player.SetRole(role);
        player.RpcSetRole(role);
    }
}