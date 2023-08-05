using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Patches.Actions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using Lotus.Server.Interfaces;

namespace Lotus.Server.Handlers;

internal class RemoveProtectHandlers
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(RemoveProtectHandlers));

    public static IRemoveProtectHandler StandardHandler = new Standard();
    public static IRemoveProtectHandler ProtectionPatchedHandler = new ProtectionPatched();

    private class ProtectionPatched : IRemoveProtectHandler
    {
        public void RemoveProtection(PlayerControl target)
        {
            if (Game.State is GameState.InMeeting) return;
            PlayerControl __instance = MurderPatches.LastAttacker;

            if (!AmongUsClient.Instance.AmHost) return;
            if (__instance == null || target == null) return;

            log.Debug($"Attack detected: {__instance.GetNameWithRole()} => {target.GetNameWithRole()}", "CheckMurder-RemoveProtectionFix");

            if (target.Data == null || target.inVent || target.inMovingPlat)
            {
                log.Trace($"Unable to kill {target.name}. Invalid Status", "CheckMurder");
                return;
            }
            if (!target.IsAlive())
            {
                log.Trace($"Unable to kill {target.name}. Player is already dead.", "CheckMurder");
                return;
            }
            if (MeetingHud.Instance != null)
            {
                log.Trace($"Unable to kill {target.name}. There is currently a meeting.", "CheckMurder");
                return;
            }

            if (__instance.PlayerId == target.PlayerId) return;

            ActionHandle handle = ActionHandle.NoInit();
            __instance.Trigger(LotusActionType.Attack, ref handle, target);

            if (target.IsAlive()) target.RpcProtectPlayer(target, 0);
        }
    }

    private class Standard : IRemoveProtectHandler
    {
        public void RemoveProtection(PlayerControl player)
        {
        }
    }
}