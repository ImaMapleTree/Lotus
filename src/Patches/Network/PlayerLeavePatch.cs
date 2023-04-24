using HarmonyLib;
using InnerNet;
using TOHTOR.API;
using TOHTOR.API.Reactive;
using TOHTOR.API.Reactive.HookEvents;
using TOHTOR.Gamemodes;
using TOHTOR.Managers;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using VentLib.Logging;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Patches.Network;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnDisconnected))]
class OnDisconnectedPatch
{
    public static void Postfix(AmongUsClient __instance)
    {
        TOHPlugin.VisibleTasksCount = false;
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
class OnPlayerLeftPatch
{
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData data, [HarmonyArgument(1)] DisconnectReasons reason)
    {
        VentLogger.Old($"{data.PlayerName}(ClientID:{data.Id})が切断(理由:{reason}, ping:{AmongUsClient.Instance.Ping})", "Session");
        if (Game.State is GameState.InLobby) return;
        Game.Players.Remove(data.Character.PlayerId);
        AntiBlackout.OnDisconnect(data.Character.Data);

        ActionHandle uselessHandle = ActionHandle.NoInit();
        if (Game.State is not GameState.InLobby) PlayerControl.AllPlayerControls.ToArray().Trigger(RoleActionType.OnDisconnect, ref uselessHandle, data.Character);
        Hooks.PlayerHooks.PlayerLeaveHook.Propagate(new PlayerHookEvent(data.Character));
        Game.CurrentGamemode.Trigger(GameAction.GameLeave, data);
    }
}