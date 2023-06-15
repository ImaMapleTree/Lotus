using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Logging;
using Lotus.Managers.Templates.Models;
using VentLib.Logging;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers.Templates;

public class TemplateTriggers
{
    public static Dictionary<string, TriggerBinder> TriggerHooks = new()
    {
        { "PlayerDeath", (key, action) => Hooks.PlayerHooks.PlayerDeathHook.Bind(key, action, true) },
        { "PlayerExiled", (key, action) => Hooks.PlayerHooks.PlayerExiledHook.Bind(key, action, true) },
        { "PlayerDisconnect", (key, action) => Hooks.PlayerHooks.PlayerDisconnectHook.Bind(key, action, true) },
        { "PlayerChat", (key, action) => Hooks.PlayerHooks.PlayerMessageHook.Bind(key, action, true) },
        { "StatusReceived", (key, action) => Hooks.PlayerHooks.PlayerMessageHook.Bind(key, action, true) },
        { "TaskComplete", (key, action) => Hooks.PlayerHooks.PlayerTaskCompleteHook.Bind(key, action, true) },
    };

    public static Dictionary<Type, Func<IHookEvent, ResolvedTrigger>> TriggerResolvers = new()
    {
        { typeof(PlayerMessageHookEvent), he => ResultFromPlayerMessageHook((PlayerMessageHookEvent)he) },

        { typeof(PlayerStatusReceivedHook), he => ResultFromPlayerStatusHook((PlayerStatusReceivedHook)he) },

        { typeof(PlayerTaskHookEvent), he => ResultFromPlayerTaskHook((PlayerTaskHookEvent)he) },

        { typeof(PlayerDeathHookEvent), he => ResultFromPlayerHook((PlayerHookEvent)he) },
        { typeof(PlayerHookEvent), he => ResultFromPlayerHook((PlayerHookEvent)he) },
    };

    public static ResolvedTrigger ResultFromPlayerStatusHook(PlayerStatusReceivedHook playerHookEvent)
    {
        return new ResolvedTrigger { Player = playerHookEvent.Player, Data = playerHookEvent.Status.Name };
    }

    public static ResolvedTrigger ResultFromPlayerTaskHook(PlayerTaskHookEvent playerHookEvent)
    {
        return new ResolvedTrigger { Player = playerHookEvent.Player, Data = playerHookEvent.Player.Data.Tasks.ToArray().Count(t => t.Complete).ToString() };
    }

    public static ResolvedTrigger ResultFromPlayerMessageHook(PlayerMessageHookEvent playerHookEvent)
    {
        return new ResolvedTrigger { Player = playerHookEvent.Player, Data = playerHookEvent.Message };
    }

    public static ResolvedTrigger ResultFromPlayerHook(PlayerHookEvent playerHookEvent)
    {
        return new ResolvedTrigger { Player = playerHookEvent.Player, Data = playerHookEvent.Player.name };
    }

    public static Hook? BindTrigger(string key, string trigger, Action<ResolvedTrigger?> handler)
    {
        return TriggerHooks.GetOptional(trigger).Transform(tb => tb(key, h =>
        {
            DevLogger.Log($"Event: {h}");
            DevLogger.Log($"Event Type: {h.GetType()}");
            handler(TriggerResolvers.GetValueOrDefault(h.GetType())?.Invoke(h));
        }), () =>
        {
            VentLogger.Warn($"Could not bind Trigger \"{key}.\" No such trigger exists!");
            return null!;
        });
    }

    public delegate Hook TriggerBinder(string key, Action<IHookEvent> handler);
}