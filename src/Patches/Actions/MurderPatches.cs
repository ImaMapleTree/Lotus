using HarmonyLib;
using Hazel;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.API.Reactive;
using TOHTOR.API.Reactive.HookEvents;
using TOHTOR.Extensions;
using TOHTOR.Gamemodes;
using TOHTOR.Managers.History.Events;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using VentLib.Logging;
using VentLib.Networking.RPC;
using VentLib.Utilities;

namespace TOHTOR.Patches.Actions;

public static class MurderPatches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
    public static class CheckMurderPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return false;
            if (__instance == null || target == null) return false;

            VentLogger.Old($"{__instance.GetNameWithRole()} => {target.GetNameWithRole()}", "CheckMurder");
            if (Game.CurrentGamemode.IgnoredActions().HasFlag(GameAction.KillPlayers)) return false;

            //死人はキルできない
            if (__instance.Data.IsDead)
            {
                VentLogger.Old($"{__instance.GetNameWithRole()}は死亡しているためキャンセルされました。", "CheckMurder");
                return false;
            }

            //不正キル防止処理
            if (target.Data == null || target.inVent || target.inMovingPlat)
            {
                VentLogger.Old("targetは現在キルできない状態です。", "CheckMurder");
                return false;
            }
            if (target.Data.IsDead) //同じtargetへの同時キルをブロック
            {
                VentLogger.Old("targetは既に死んでいたため、キルをキャンセルしました。", "CheckMurder");
                return false;
            }
            if (MeetingHud.Instance != null) //会議中でないかの判定
            {
                VentLogger.Old("会議が始まっていたため、キルをキャンセルしました。", "CheckMurder");
                return false;
            }

            if (__instance.PlayerId == target.PlayerId) return false;

            ActionHandle handle = ActionHandle.NoInit();
            __instance.Trigger(RoleActionType.Attack, ref handle, target);
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public class MurderPlayerPatch
    {
        public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            VentLogger.Old($"{__instance.GetNameWithRole()} => {target.GetNameWithRole()}{(target.protectedByGuardian ? "(Protected)" : "")}", "MurderPlayer");
        }
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (!target.Data.IsDead) return;

            IDeathEvent deathEvent = Game.GameHistory.GetCauseOfDeath(target.PlayerId)
                .OrElseGet(() => __instance.PlayerId == target.PlayerId
                    ? new SuicideEvent(__instance)
                    : new DeathEvent(target, __instance)
                );

            Game.GameHistory.AddEvent(deathEvent);
            Game.GameHistory.SetCauseOfDeath(target.PlayerId, deathEvent);


            ActionHandle ignored = ActionHandle.NoInit();
            target.Trigger(RoleActionType.MyDeath, ref ignored, __instance);
            Game.TriggerForAll(RoleActionType.AnyDeath, ref ignored, target, __instance);

            Hooks.PlayerHooks.PlayerMurderHook.Propagate(new PlayerMurderHookEvent(__instance, target));
            Hooks.PlayerHooks.PlayerDeathHook.Propagate(new PlayerHookEvent(target));
        }
    }
}