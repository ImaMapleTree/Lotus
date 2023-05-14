using System.Linq;
using HarmonyLib;
using Lotus.API.Reactive.HookEvents;
using VentLib.Networking.RPC;
using VentLib.Utilities.Extensions;

namespace Lotus.API.Reactive;

public class Hooks
{
    // TODO: Allow for async hooking

    public static class GameStateHooks
    {
        public static readonly Hook<GameStateHookEvent> GameStartHook = new SimpleHook<GameStateHookEvent>();
        public static readonly Hook<GameStateHookEvent> RoundStartHook = new SimpleHook<GameStateHookEvent>();
        public static readonly Hook<GameStateHookEvent> RoundEndHook = new SimpleHook<GameStateHookEvent>();
        public static readonly Hook<GameStateHookEvent> GameEndHook = new SimpleHook<GameStateHookEvent>();
    }

    public static class NetworkHooks
    {
        public static readonly Hook<RpcHookEvent> RpcHook = new SimpleHook<RpcHookEvent>();

        static NetworkHooks()
        {
            RpcMeta.AddSubscriber(meta => RpcHook.Propagate(new RpcHookEvent(meta)));
        }
    }

    public static class PlayerHooks
    {
        public static readonly Hook<PlayerHookEvent> PlayerJoinHook = new SimpleHook<PlayerHookEvent>();
        public static readonly Hook<PlayerHookEvent> PlayerLeaveHook = new SimpleHook<PlayerHookEvent>();

        public static readonly Hook<PlayerMessageHookEvent> PlayerMessageHook = new SimpleHook<PlayerMessageHookEvent>();
        public static readonly Hook<PlayerActionHookEvent> PlayerActionHook = new SimpleHook<PlayerActionHookEvent>();

        public static readonly Hook<PlayerMurderHookEvent> PlayerMurderHook = new SimpleHook<PlayerMurderHookEvent>();
        public static readonly Hook<PlayerHookEvent> PlayerDeathHook = new SimpleHook<PlayerHookEvent>();
        public static readonly Hook<PlayerHookEvent> PlayerExiledHook = new SimpleHook<PlayerHookEvent>();
    }

    public static class MeetingHooks
    {
        public static readonly Hook<MeetingHookEvent> MeetingCalledHook = new SimpleHook<MeetingHookEvent>();
    }

    public static class SabotageHooks
    {
        public static readonly Hook<SabotageHookEvent> SabotageCalledHook = new SimpleHook<SabotageHookEvent>();
        public static readonly Hook<SabotageHookEvent> SabotagePartialFixHook = new SimpleHook<SabotageHookEvent>();
        public static readonly Hook<SabotageFixHookEvent> SabotageFixedHook = new SimpleHook<SabotageFixHookEvent>();
    }

    public static void UnbindAll(string hookKey)
    {
        typeof(Hooks).GetNestedTypes().AddItem(typeof(Hooks))
            .SelectMany(type => type.GetFields())
            .Select(f => f.GetValue(null))
            .OfType<Hook>()
            .ForEach(hook => hook.Unbind(hookKey));
    }
}