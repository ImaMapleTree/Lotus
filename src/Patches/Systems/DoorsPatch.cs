using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.API.Vanilla.Sabotages;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;

namespace Lotus.Patches.Systems;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
public class DoorsPatch
{
    public static bool Prefix(ShipStatus __instance, [HarmonyArgument(0)] SystemTypes room)
    {

        ISabotage sabotage = new DoorSabotage(room);

        ActionHandle handle = ActionHandle.NoInit();
        Game.TriggerForAll(LotusActionType.SabotageStarted, ref handle, sabotage, PlayerControl.LocalPlayer);
        if (handle.IsCanceled) return false;

        Hooks.SabotageHooks.SabotageCalledHook.Propagate(new SabotageHookEvent(sabotage));
        return true;
    }
}