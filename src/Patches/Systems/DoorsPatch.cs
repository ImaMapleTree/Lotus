using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.API.Vanilla.Sabotages;
using Lotus.Gamemodes;
using Lotus.Options;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.API;

namespace Lotus.Patches.Systems;

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