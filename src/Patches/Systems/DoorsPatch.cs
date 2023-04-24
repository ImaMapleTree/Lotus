using HarmonyLib;
using TOHTOR.API;
using TOHTOR.API.Reactive;
using TOHTOR.API.Reactive.HookEvents;
using TOHTOR.API.Vanilla.Sabotages;
using TOHTOR.Gamemodes;
using TOHTOR.Options;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;

namespace TOHTOR.Patches.Systems;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
public class DoorsPatch
{
    public static bool Prefix(ShipStatus __instance, [HarmonyArgument(0)] SystemTypes room)
    {
        if (Game.CurrentGamemode.IgnoredActions().HasFlag(GameAction.CloseDoors)) return false;
        if (GeneralOptions.SabotageOptions.DisabledSabotages.HasFlag(SabotageType.Door)) return false;

        ISabotage sabotage = new DoorSabotage(room);

        ActionHandle handle = ActionHandle.NoInit();
        Game.TriggerForAll(RoleActionType.SabotageStarted, ref handle, sabotage, PlayerControl.LocalPlayer);
        if (handle.IsCanceled) return false;

        Hooks.SabotageHooks.SabotageCalledHook.Propagate(new SabotageHookEvent(sabotage));
        return true;
    }
}