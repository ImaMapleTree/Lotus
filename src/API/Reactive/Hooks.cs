using TOHTOR.API.Reactive.HookEvents;
using VentLib.Networking.RPC;

namespace TOHTOR.API.Reactive;

public class Hooks
{
    // TODO: Allow for async hooking

    public static class GameStateHooks
    {
        public static readonly Hook<GameStateHookEvent> GameStartHook = new SimpleHook<GameStateHookEvent>();
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
    }

    public static class MeetingHooks
    {
        public static readonly Hook<MeetingHookEvent> MeetingCalledHook = new SimpleHook<MeetingHookEvent>();
    }
}