using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using Lotus.API.Odyssey;
using Lotus.Roles;
using Lotus.Roles.Internals;
using Lotus.API.Stats;
using Lotus.Extensions;
using Lotus.Roles.Internals.Enums;
using UnityEngine;
using VentLib.Networking.RPC;
using VentLib.Utilities;

namespace Lotus.Patches.Actions;

[HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
class EnterVentPatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(EnterVentPatch));

    internal static Dictionary<byte, Vector2?> LastVentLocation = new();

    public static void Postfix(Vent __instance, [HarmonyArgument(0)] PlayerControl pc)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        log.Trace($"{pc.GetNameWithRole()} Entered Vent (ID: {__instance.Id})", "CoEnterVent");
        CustomRole role = pc.GetCustomRole();
        ActionHandle vented = ActionHandle.NoInit();
        pc.Trigger(LotusActionType.MyEnterVent, ref vented, __instance);

        if (!role.CanVent())
        {
            log.Trace($"{pc.GetNameWithRole()} cannot enter vent. Booting.");
            Async.Schedule(() => pc.MyPhysics.RpcBootFromVent(__instance.Id), 0.01f);
            return;
        }

        if (vented.IsCanceled) {
            log.Trace($"{pc.GetNameWithRole()} vent action got canceled. Booting.");
            Async.Schedule(() => pc.MyPhysics.RpcBootFromVent(__instance.Id), 0.4f);
            return;
        }

        vented = ActionHandle.NoInit();
        Game.TriggerForAll(LotusActionType.AnyEnterVent, ref vented, __instance, pc);
        if (vented.IsCanceled) Async.Schedule(() => pc.MyPhysics.RpcBootFromVent(__instance.Id), 0.4f);
        else VanillaStatistics.TimesVented.Update(pc.PlayerId, i => i + 1);
    }
}

[HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
class ExitVentPatch
{
    public static void Postfix(Vent __instance, [HarmonyArgument(0)] PlayerControl pc)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        ActionHandle exitVent = ActionHandle.NoInit();
        pc.Trigger(LotusActionType.VentExit, ref exitVent, __instance);
        if (exitVent.IsCanceled) Async.Schedule(() => RpcV3.Immediate(pc.MyPhysics.NetId, RpcCalls.EnterVent, SendOption.None).WritePacked(__instance.Id).Send(), 0.5f);
        else EnterVentPatch.LastVentLocation.Remove(pc.PlayerId);
    }
}
