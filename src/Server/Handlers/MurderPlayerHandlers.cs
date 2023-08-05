using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Extensions;
using Lotus.Managers.History.Events;
using Lotus.Patches.Actions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using Lotus.Server.Interfaces;
using VentLib.Utilities;
using VentLib.Utilities.Optionals;

namespace Lotus.Server.Handlers;

internal class MurderPlayerHandlers
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(MurderPlayerHandlers));

    public static IMurderPlayerHandler StandardHandler = new Standard();

    private class Standard: IMurderPlayerHandler
    {
        public void MurderPlayer(PlayerControl __instance, PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (!target.Data.IsDead) return;
            MurderPatches.Lock(__instance.PlayerId);

            log.Trace($"{__instance.GetNameWithRole()} => {target.GetNameWithRole()}{(target.protectedByGuardian ? "(Protected)" : "")}", "MurderPlayer");

            IDeathEvent deathEvent = Game.MatchData.GameHistory.GetCauseOfDeath(target.PlayerId)
                .OrElseGet(() => __instance.PlayerId == target.PlayerId
                    ? new SuicideEvent(__instance)
                    : new DeathEvent(target, __instance)
                );

            Game.MatchData.GameHistory.AddEvent(deathEvent);
            Game.MatchData.GameHistory.SetCauseOfDeath(target.PlayerId, deathEvent);


            ActionHandle ignored = ActionHandle.NoInit();
            target.Trigger(LotusActionType.MyDeath, ref ignored, __instance, deathEvent.Instigator(), deathEvent);
            Game.TriggerForAll(LotusActionType.AnyDeath, ref ignored, target, __instance, deathEvent.Instigator(), deathEvent);

            PlayerControl killer = deathEvent.Instigator().FlatMap(k => new UnityOptional<PlayerControl>(k.MyPlayer)).OrElse(__instance);
            PlayerMurderHookEvent playerMurderHookEvent = new(killer, target, deathEvent);
            Hooks.PlayerHooks.PlayerMurderHook.Propagate(playerMurderHookEvent);
            Hooks.PlayerHooks.PlayerDeathHook.Propagate(playerMurderHookEvent);

            Async.Schedule(() =>
            {
                if (target != null) target.SetChatName(target.name);
            }, 0.1f);
        }
    }
}