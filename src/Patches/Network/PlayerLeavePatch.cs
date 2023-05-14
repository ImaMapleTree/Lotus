using System.Collections.Generic;
using HarmonyLib;
using InnerNet;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Gamemodes;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using VentLib.Logging;

namespace Lotus.Patches.Network;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnDisconnected))]
class OnDisconnectedPatch
{
    public static void Postfix(AmongUsClient __instance)
    {
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
class OnPlayerLeftPatch
{
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData data, [HarmonyArgument(1)] DisconnectReasons reason)
    {
        VentLogger.Debug($"{data.PlayerName} (ClientID={data.Id}) left the game. (Reason={reason})", "SessionEnd");
        if (Game.State is GameState.InLobby) return;
        Game.NameModels.Remove(data.Character.PlayerId);

        ActionHandle uselessHandle = ActionHandle.NoInit();
        if (Game.State is not GameState.InLobby)
        {
            PlayerControl.AllPlayerControls.ToArray().Trigger(RoleActionType.Disconnect, ref uselessHandle, data.Character);
            Game.MatchData.Roles.MainRoles.GetValueOrDefault(data.Character.PlayerId)?.HandleDisconnect();
            Game.MatchData.Roles.SubRoles.GetValueOrDefault(data.Character.PlayerId)?.ForEach(r => r.HandleDisconnect());
        }
        Hooks.PlayerHooks.PlayerLeaveHook.Propagate(new PlayerHookEvent(data.Character));
        Game.CurrentGamemode.Trigger(GameAction.GameLeave, data);
    }
}