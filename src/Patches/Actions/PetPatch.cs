using System;
using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using Lotus.API.Odyssey;
using Lotus.Roles.Internals;
using Lotus.Extensions;
using Lotus.Roles.Internals.Enums;
using VentLib.Logging;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.Patches.Actions;

public class PetPatch
{
    public const float PetDelay = 0.4f;
    private const byte PetCallId = (byte)RpcCalls.Pet;

    private static readonly Dictionary<byte, DateTime> LastPet = new();
    private static readonly Dictionary<byte, int> TimesPet = new();

    [QuickPostfix(typeof(PlayerControl), nameof(PlayerControl.TryPet))]
    public static void InterceptHostPet(PlayerControl __instance) => InterceptPet(__instance.MyPhysics, PetCallId);

    [QuickPostfix(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
    public static void InterceptPet(PlayerPhysics __instance, [HarmonyArgument(0)] byte callId)
    {
        if (!AmongUsClient.Instance.AmHost || callId != PetCallId) return;

        byte playerId = __instance.myPlayer.PlayerId;

        Async.Schedule(() =>
        {
            __instance.CancelPet();
            RpcV3.Immediate(__instance.NetId, RpcCalls.CancelPet, SendOption.None).Send();
        }, NetUtils.DeriveDelay(PetDelay));

        if (DateTime.Now.Subtract(LastPet.GetValueOrDefault(playerId)).TotalSeconds < PetDelay) return;

        RpcV3.Immediate(__instance.NetId, RpcCalls.CancelPet, SendOption.None).Send();

        LastPet[playerId] = DateTime.Now;
        int timesPet = TimesPet[playerId] = TimesPet.GetValueOrDefault(playerId) + 1;
        PlayerControl player = __instance.myPlayer;

        Async.Schedule(() => ClearPetHold(player, timesPet), NetUtils.DeriveDelay(0.5f, 0.005f));

        VentLogger.Trace($"{player.name} => Pet", "PetPatch");
        ActionHandle handle = ActionHandle.NoInit();
        Game.TriggerForAll(LotusActionType.AnyPet, ref handle, player);
        player.Trigger(LotusActionType.OnPet, ref handle, __instance);

        handle = ActionHandle.NoInit();
        player.Trigger(LotusActionType.OnHoldPet, ref handle, __instance, timesPet);

    }

    private static void ClearPetHold(PlayerControl player, int currentTimes)
    {
        int timesHeld = TimesPet[player.PlayerId];
        if (timesHeld != currentTimes) return;

        TimesPet[player.PlayerId] = 0;
        ActionHandle handle = ActionHandle.NoInit();
        player.Trigger(LotusActionType.OnPetRelease, ref handle, timesHeld);
    }
}