using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Extensions;
using Lotus.RPC;
using VentLib.Logging;
using VentLib.Utilities;
using Priority = HarmonyLib.Priority;

namespace Lotus.Patches.Actions;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public static class ShapeshiftPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        string invokerName = new StackTrace(5)?.GetFrame(0)?.GetMethod()?.Name;
        VentLogger.Debug($"Shapeshift Cause (Invoker): {invokerName}", "ShapeshiftEvent");
        if (invokerName is "RpcShapeshiftV2" or "RpcRevertShapeshiftV2" or "<Shapeshift>b__0" or "<RevertShapeshift>b__0") return true;
        if (invokerName is "CRpcShapeshift" or "CRpcRevertShapeshift" or "<Shapeshift>b__0" or "<RevertShapeshift>b__0") return true;
        VentLogger.Info($"{__instance?.GetNameWithRole()} => {target?.GetNameWithRole()}", "Shapeshift");
        if (!AmongUsClient.Instance.AmHost) return true;

        var shapeshifter = __instance;
        var shapeshifting = shapeshifter.PlayerId != target.PlayerId;


        ActionHandle handle = ActionHandle.NoInit();
        __instance.Trigger(shapeshifting ? RoleActionType.Shapeshift : RoleActionType.Unshapeshift, ref handle, target);

        if (handle.IsCanceled)
        {
            Async.Schedule(() => __instance.CRpcShapeshift(__instance, false), NetUtils.DeriveDelay(1.2f));
            return false;
        }

        Game.TriggerForAll(shapeshifting ? RoleActionType.AnyShapeshift : RoleActionType.AnyUnshapeshift, ref handle, __instance, target);
        if (!handle.IsCanceled)
        {
            Hooks.PlayerHooks.PlayerShapeshiftHook.Propagate(new PlayerShapeshiftHookEvent(__instance, target.Data, !shapeshifting));
            return true;
        }
        Async.Schedule(() => __instance.CRpcShapeshift(__instance, false), NetUtils.DeriveDelay(1.2f));
        return false;
    }
}

[HarmonyPriority(Priority.LowerThanNormal)]
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public static class ShapeshiftFixPatch
{
    private static Dictionary<byte, byte> _shapeshifted = new();

    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        if (target.PlayerId == __instance.PlayerId)
            _shapeshifted.Remove(__instance.PlayerId);
        else _shapeshifted[__instance.PlayerId] = target.PlayerId;

        Async.Schedule(() => Game.RenderAllForAll(force: true), NetUtils.DeriveDelay(0.1f));
    }

    public static bool IsShapeshifted(this PlayerControl player) => _shapeshifted.ContainsKey(player.PlayerId);
    public static byte GetShapeshifted(this PlayerControl player) => _shapeshifted.GetValueOrDefault(player.PlayerId, (byte)255);
}