using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Roles.Internals;
using Lotus.Extensions;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using Lotus.RPC;
using VentLib.Utilities;
using Priority = HarmonyLib.Priority;

namespace Lotus.Patches.Actions;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public static class ShapeshiftPatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(ShapeshiftPatch));

    public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        string invokerName = new StackTrace(5)?.GetFrame(0)?.GetMethod()?.Name;
        log.Debug($"Shapeshift Cause (Invoker): {invokerName}", "ShapeshiftEvent");
        if (invokerName is "RpcShapeshiftV2" or "RpcRevertShapeshiftV2" or "<Shapeshift>b__0" or "<RevertShapeshift>b__0") return;
        if (invokerName is "CRpcShapeshift" or "CRpcRevertShapeshift" or "<Shapeshift>b__0" or "<RevertShapeshift>b__0") return;
        log.Info($"{__instance?.GetNameWithRole()} => {target?.GetNameWithRole()}", "Shapeshift");
        if (!AmongUsClient.Instance.AmHost) return;

        var shapeshifter = __instance;
        var shapeshifting = shapeshifter.PlayerId != target.PlayerId;


        ActionHandle handle = ActionHandle.NoInit();
        __instance.Trigger(shapeshifting ? LotusActionType.Shapeshift : LotusActionType.Unshapeshift, ref handle, target);

        if (handle.IsCanceled) return;

        Game.TriggerForAll(shapeshifting ? LotusActionType.AnyShapeshift : LotusActionType.AnyUnshapeshift, ref handle, __instance, target);

        if (handle.IsCanceled) return;

        Hooks.PlayerHooks.PlayerShapeshiftHook.Propagate(new PlayerShapeshiftHookEvent(__instance, target.Data, !shapeshifting));
    }
}

[HarmonyPriority(Priority.LowerThanNormal)]
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public static class ShapeshiftFixPatch
{
    private static Dictionary<byte, byte> _shapeshifted = new();

    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        if (target.PlayerId == __instance.PlayerId) _shapeshifted.Remove(__instance.PlayerId);
        else _shapeshifted[__instance.PlayerId] = target.PlayerId;
        Async.Schedule(() =>
        {
            if (Game.State is not GameState.InMeeting) __instance.NameModel().Render(force: true);
        }, 1.2f);
    }

    public static bool IsShapeshifted(this PlayerControl player) => _shapeshifted.ContainsKey(player.PlayerId);
    public static byte GetShapeshifted(this PlayerControl player) => _shapeshifted.GetValueOrDefault(player.PlayerId, (byte)255);
}