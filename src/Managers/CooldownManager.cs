using System.Collections.Concurrent;
using System.Collections.Generic;
using Lotus.API.Reactive;
using Lotus.GUI;
using Lotus.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Attributes;

namespace Lotus.Managers;

[LoadStatic]
internal static class CooldownManager
{
    private static readonly Queue<Cooldown> Cooldowns = new();
    private static readonly ConcurrentQueue<Cooldown> WaitForInsertionQueue = new();

    static CooldownManager()
    {
        Hooks.GameStateHooks.GameStartHook.Bind(typeof(CooldownManager), ev => ev.CurrentGameMode.CoroutineManager.CreateLoop(DoCooldownLoop));
        Hooks.GameStateHooks.GameEndHook.Bind(typeof(CooldownManager), ClearCooldowns);
    }

    public static void SubmitCooldown(Cooldown cooldown)
    {
        WaitForInsertionQueue.Enqueue(cooldown);
    }

    private static float DoCooldownLoop()
    {
        int size = Cooldowns.Count;

        while (!WaitForInsertionQueue.IsEmpty)
        {
            if (WaitForInsertionQueue.TryDequeue(out Cooldown? result)) Cooldowns.Enqueue(result);
        }

        for (int i = 0; i < size; i++)
        {
            Cooldown cooldown = Cooldowns.Dequeue();
            if (!cooldown.IsReady()) Cooldowns.Enqueue(cooldown);
        }

        return 0.1f;
    }

    private static void ClearCooldowns()
    {
        WaitForInsertionQueue.Clear();
        Cooldowns.Clear();
    }
}