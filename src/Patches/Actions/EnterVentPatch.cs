using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using Lotus.API.Odyssey;
using Lotus.Gamemodes;
using Lotus.Roles;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.API;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Logging;
using VentLib.Networking.RPC;
using VentLib.Utilities;

namespace Lotus.Patches.Actions;

[HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
class EnterVentPatch
{
    internal static Dictionary<byte, Vector2?> lastVentLocation = new();

    public static void Postfix(Vent __instance, [HarmonyArgument(0)] PlayerControl pc)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        VentLogger.Trace($"{pc.GetNameWithRole()} Entered Vent (ID: {__instance.Id})", "CoEnterVent");
        CustomRole role = pc.GetCustomRole();
        if (Game.CurrentGamemode.IgnoredActions().HasFlag(GameAction.EnterVent)) pc.MyPhysics.RpcBootFromVent(__instance.Id);
        ActionHandle vented = ActionHandle.NoInit();
        pc.Trigger(RoleActionType.MyEnterVent, ref vented, __instance);

        if (!role.CanVent() || vented.IsCanceled) {
            VentLogger.Trace($"{pc.GetNameWithRole()} cannot enter vent. Booting.");
            Async.Schedule(() => pc.MyPhysics.RpcBootFromVent(__instance.Id), 0.4f);
            return;
        }

        vented = ActionHandle.NoInit();
        Game.TriggerForAll(RoleActionType.AnyEnterVent, ref vented, __instance, pc);
        if (vented.IsCanceled)
            Async.Schedule(() => pc.MyPhysics.RpcBootFromVent(__instance.Id), 0.4f);
        else lastVentLocation[pc.PlayerId] = new Vector2(__instance.Offset.x, __instance.Offset.y);
    }
}

[HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
class ExitVentPatch
{
    public static void Postfix(Vent __instance, [HarmonyArgument(0)] PlayerControl pc)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        ActionHandle exitVent = ActionHandle.NoInit();
        pc.Trigger(RoleActionType.VentExit, ref exitVent, __instance);
        //if (exitVent.IsCanceled) Async.Schedule(() => pc.MyPhysics.RpcEnterVent(__instance.Id), 0.0f);
        if (exitVent.IsCanceled) Async.Schedule(() => RpcV3.Immediate(pc.MyPhysics.NetId, RpcCalls.EnterVent, SendOption.None).WritePacked(__instance.Id).Send(), 0.5f);
        else EnterVentPatch.lastVentLocation.Remove(pc.PlayerId);
    }
}