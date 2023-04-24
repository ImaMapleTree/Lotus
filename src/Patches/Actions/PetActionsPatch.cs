using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using TOHTOR.Extensions;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using VentLib.Logging;
using VentLib.Networking.RPC;
using VentLib.Utilities;

namespace TOHTOR.Patches.Actions;


[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.TryPet))]
class LocalPetPatch
{
    private static bool _hostPetBuffer;
    public static bool Prefix(PlayerControl __instance)
    {
        if (!(AmongUsClient.Instance.AmHost)) return true;
        if (_hostPetBuffer) return false;
        ExternalRpcPetPatch.Prefix(__instance.MyPhysics, 51, new MessageReader());
        _hostPetBuffer = true;
        Async.Schedule(() => _hostPetBuffer = false, 0.4f);
        return false;
    }

    public static void Postfix(PlayerControl __instance)
    {
        __instance.MyPhysics.CancelPet();
        __instance.petting = false;
    }

}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
class ExternalRpcPetPatch
{

    private static readonly Dictionary<byte, int> timesPet = new();

    public static void Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        var rpcType = callId == 51 ? RpcCalls.Pet : (RpcCalls)callId;
        if (rpcType != RpcCalls.Pet) return;

        PlayerControl playerControl = __instance.myPlayer;

        if (AmongUsClient.Instance.AmHost) __instance.CancelPet();

        Async.Schedule(() => RpcV2.Immediate(__instance.NetId, RpcCalls.CancelPet, SendOption.None).Send(), 0.4f);
        int currentTimes = timesPet[playerControl.PlayerId] = timesPet.GetValueOrDefault(playerControl.PlayerId) + 1;
        Async.Schedule(() => ClearPetHold(playerControl, currentTimes), NetUtils.DeriveDelay(0.5f, 0.005f));

        VentLogger.Trace($"{playerControl.UnalteredName()} => Pet", "PetPatch");
        ActionHandle handle = ActionHandle.NoInit();
        playerControl.Trigger(RoleActionType.OnPet, ref handle, __instance);

        handle = ActionHandle.NoInit();
        playerControl.Trigger(RoleActionType.OnHoldPet, ref handle, __instance, currentTimes);
    }

    private static void ClearPetHold(PlayerControl player, int currentTimes)
    {
        int timesHeld = timesPet[player.PlayerId];
        if (timesHeld != currentTimes) return;
        timesPet[player.PlayerId] = 0;
        ActionHandle handle = ActionHandle.NoInit();
        player.Trigger(RoleActionType.OnPetRelease, ref handle, timesHeld);
    }
}




















/*
*/