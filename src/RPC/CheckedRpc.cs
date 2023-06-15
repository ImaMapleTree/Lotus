using AmongUs.GameOptions;
using Hazel;
using Lotus.API.Odyssey;
using Lotus.Extensions;
using VentLib.Logging;
using VentLib.Networking.RPC;
using VentLib.Utilities.Extensions;

namespace Lotus.RPC;

public static class CheckedRpc
{
    // TODO Shapeshift queue so that i dont need to stacktrace shapeshifting
    public static void CRpcShapeshift(this PlayerControl player, PlayerControl target, bool animate)
    {
        if (!player.IsAlive()) return;
        if (AmongUsClient.Instance.AmClient) player.Shapeshift(target, animate);

        RpcV3.Mass(SendOption.Reliable)
            .Start(player.NetId, RpcCalls.Shapeshift).Write(target).Write(animate).End()
            .Send();
    }

    public static void CRpcRevertShapeshift(this PlayerControl player, bool animate)
    {
        VentLogger.Trace("CRevertShapeshift");
        if (!player.IsAlive()) return;
        if (AmongUsClient.Instance.AmClient) player.Shapeshift(player, animate);
        RpcV3.Mass(SendOption.Reliable)
            .Start(player.NetId, RpcCalls.Shapeshift).Write(player).Write(animate).End()
            .Send();
        player.NameModel().Render(sendToPlayer: true, force: true);
    }

    public static void CRpcSetRole(this PlayerControl player, RoleTypes role)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (player.IsHost()) player.SetRole(role);
        RpcV3.Immediate(player.NetId, RpcCalls.SetRole).Write((ushort)role).Send();
    }
}