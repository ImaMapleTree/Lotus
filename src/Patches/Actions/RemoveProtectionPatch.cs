/*using HarmonyLib;
using Lotus.Extensions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using VentLib.Logging;

namespace Lotus.Patches.Actions;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RemoveProtection))]
public class RemoveProtectionPatch
{
    public static void Postfix(PlayerControl __instance)
    {
        PlayerControl target = __instance;
        __instance = MurderPatches.LastAttacker;

        if (!AmongUsClient.Instance.AmHost) return;
        if (__instance == null || target == null) return;

        VentLogger.Debug($"{__instance.GetNameWithRole()} => {target.GetNameWithRole()}", "CheckMurder-RemoveProtectionFix");

        if (target.Data == null || target.inVent || target.inMovingPlat)
        {
            VentLogger.Trace($"Unable to kill {target.name}. Invalid Status", "CheckMurder");
            return;
        }
        if (!target.IsAlive())
        {
            VentLogger.Trace($"Unable to kill {target.name}. Player is already dead.", "CheckMurder");
            return;
        }
        if (MeetingHud.Instance != null)
        {
            VentLogger.Trace($"Unable to kill {target.name}. There is currently a meeting.", "CheckMurder");
            return;
        }

        if (__instance.PlayerId == target.PlayerId) return;

        ActionHandle handle = ActionHandle.NoInit();
        __instance.Trigger(LotusActionType.Attack, ref handle, target);

        if (target.IsAlive()) target.RpcProtectPlayer(target, 0);
    }
}*/