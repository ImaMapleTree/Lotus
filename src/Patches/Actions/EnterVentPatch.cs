using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using Lotus.API.Odyssey;
using Lotus.Gamemodes;
using Lotus.Roles;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.API;
using Lotus.API.Stats;
using Lotus.Extensions;
using Lotus.Logging;
using UnityEngine;
using VentLib.Logging;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.Patches.Actions;

[HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
class EnterVentPatch
{
    internal static Dictionary<byte, Vector2?> LastVentLocation = new();

    public static void Postfix(Vent __instance, [HarmonyArgument(0)] PlayerControl pc)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        VentLogger.Trace($"{pc.GetNameWithRole()} Entered Vent (ID: {__instance.Id})", "CoEnterVent");
        CustomRole role = pc.GetCustomRole();
        if (Game.CurrentGamemode.IgnoredActions().HasFlag(GameAction.EnterVent)) pc.MyPhysics.RpcBootFromVent(__instance.Id);
        ActionHandle vented = ActionHandle.NoInit();
        pc.Trigger(RoleActionType.MyEnterVent, ref vented, __instance);

        DevLogger.Log($"Vent Cooldown: {AUSettings.EngineerCooldown()}");
        DevLogger.Log($"Vent Cooldown 2: {pc.Data.Role.TryCast<EngineerRole>()?.GetCooldown()}");
        if (!role.CanVent())
        {
            VentLogger.Trace($"{pc.GetNameWithRole()} cannot enter vent. Booting.");
            Async.Schedule(() => pc.MyPhysics.RpcBootFromVent(__instance.Id), 0.01f);
            return;
        }

        if (vented.IsCanceled) {
            VentLogger.Trace($"{pc.GetNameWithRole()} vent action got canceled. Booting.");
            Async.Schedule(() => pc.MyPhysics.RpcBootFromVent(__instance.Id), 0.4f);
            return;
        }

        vented = ActionHandle.NoInit();
        Game.TriggerForAll(RoleActionType.AnyEnterVent, ref vented, __instance, pc);
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
        pc.Trigger(RoleActionType.VentExit, ref exitVent, __instance);
        if (exitVent.IsCanceled) Async.Schedule(() => RpcV3.Immediate(pc.MyPhysics.NetId, RpcCalls.EnterVent, SendOption.None).WritePacked(__instance.Id).Send(), 0.5f);
        else EnterVentPatch.LastVentLocation.Remove(pc.PlayerId);
    }
}
