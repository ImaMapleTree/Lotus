using Hazel;
using TOHTOR.Extensions;
using VentLib.Networking.RPC;
using VentLib.Utilities;

namespace TOHTOR.API;

public class ProtectedRpc
{
    public static void CheckMurder(PlayerControl killer, PlayerControl target)
    {
        if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost) return;
        if (target == null) return;
        GameData.PlayerInfo data = target.Data;
        if (data == null || data.IsDead) return;

        if (MeetingHud.Instance != null)
        {
            target.RpcExileV2();
            RpcV2.Immediate(killer.NetId, RpcCalls.MurderPlayer).Write(target).Send(target.GetClientId());
            return;
        }

        if (AmongUsClient.Instance.AmClient) killer.MurderPlayer(target);
        RpcV2.Immediate(killer.NetId, RpcCalls.MurderPlayer, SendOption.None).Write(target).Send();
    }
}