using Lotus.Extensions;
using Lotus.Patches.Actions;
using VentLib.Networking.RPC;
using VentLib.Utilities;

namespace Lotus.API;

public class ProtectedRpc
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(ProtectedRpc));

    public static void CheckMurder(PlayerControl killer, PlayerControl target)
    {
        log.Trace("Protected Check Murder", "ProtectedRpc::CheckMurder");
        if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost) return;
        if (target == null) return;
        GameData.PlayerInfo data = target.Data;
        if (data == null) return;
        if (!MurderPatches.Lock(killer.PlayerId)) return;

        if (MeetingHud.Instance != null)
        {
            killer.RpcVaporize(target);
            RpcV3.Immediate(killer.NetId, RpcCalls.MurderPlayer).Write(target).Send(target.GetClientId());
            return;
        }

        // Suspicious call, perhaps figure out a better way to track if players are protected at all times
        if (AmongUsClient.Instance.AmHost) killer.MurderPlayer(target);


        RpcV3.Immediate(killer.NetId, RpcCalls.MurderPlayer).Write(target).Send();
        target.Data.IsDead = true;
    }
}