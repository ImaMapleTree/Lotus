using Lotus.Extensions;
using Lotus.Patches.Actions;
using Lotus.Server.Interfaces;
using VentLib.Networking.RPC;
using VentLib.Utilities;

namespace Lotus.Server.Handlers;

internal class RpcMarkHandlers
{
    public static IRpcMarkHandler StandardHandler = new Standard();
    public static IRpcMarkHandler ProtectionPatchedHandler = new ProtectionPatched();

    private class Standard : IRpcMarkHandler
    {
        public void RpcMark(PlayerControl killer, PlayerControl? target, int colorId = 0)
        {
            if (target == null) target = killer;
            MurderPatches.Lock(killer.PlayerId);

            // Host
            if (killer.AmOwner)
            {
                killer.ProtectPlayer(target, colorId);
                killer.MurderPlayer(target);
            }

            // Other Clients
            if (killer.PlayerId == 0) return;

            RpcV3.Mass()
                .Start(killer.NetId, RpcCalls.ProtectPlayer).Write(target).Write(colorId).End()
                .Start(killer.NetId, RpcCalls.MurderPlayer).Write(target).End()
                .Send(killer.GetClientId());
        }
    }

    private class ProtectionPatched : IRpcMarkHandler
    {
        public void RpcMark(PlayerControl killer, PlayerControl? target, int colorId = 0)
        {
            killer.GetCustomRole().RefreshKillCooldown(target);
        }
    }
}