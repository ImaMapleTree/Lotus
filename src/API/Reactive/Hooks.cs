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

    public static class ResultHooks
    {
        public static readonly Hook<WinnersHookEvent> WinnersHook = new SimpleHook<WinnersHookEvent>();
        public static readonly Hook<LosersHookEvent> LosersHook = new SimpleHook<LosersHookEvent>();
        public static readonly Hook<EmptyHookEvent> ForceEndGameHook = new SimpleHook<EmptyHookEvent>();
    }

    public static class NetworkHooks
    {
        public static readonly Hook<GameJoinHookEvent> GameJoinHook = new SimpleHook<GameJoinHookEvent>();
        public static readonly Hook<RpcHookEvent> RpcHook = new SimpleHook<RpcHookEvent>();
        public static readonly Hook<ClientConnectHookEvent> ClientConnectHook = new SimpleHook<ClientConnectHookEvent>();
        public static readonly Hook<ReceiveVersionHookEvent> ReceiveVersionHook = new SimpleHook<ReceiveVersionHookEvent>();

        static NetworkHooks()
        {
            RpcMeta.AddSubscriber(meta => RpcHook.Propagate(new RpcHookEvent(meta)));
        }
    }

    public static class PlayerHooks
    {
        public static readonly Hook<PlayerHookEvent> PlayerJoinHook = new SimpleHook<PlayerHookEvent>();
        public static readonly Hook<PlayerHookEvent> PlayerDisconnectHook = new SimpleHook<PlayerHookEvent>();

        public static readonly Hook<PlayerMessageHookEvent> PlayerMessageHook = new SimpleHook<PlayerMessageHookEvent>();
        public static readonly Hook<PlayerActionHookEvent> PlayerActionHook = new SimpleHook<PlayerActionHookEvent>();
        public static readonly Hook<PlayerTaskHookEvent> PlayerTaskCompleteHook = new SimpleHook<PlayerTaskHookEvent>();

        public static readonly Hook<PlayerMurderHookEvent> PlayerMurderHook = new SimpleHook<PlayerMurderHookEvent>();
        public static readonly Hook<PlayerDeathHookEvent> PlayerDeathHook = new SimpleHook<PlayerDeathHookEvent>();
        public static readonly Hook<PlayerShapeshiftHookEvent> PlayerShapeshiftHook = new SimpleHook<PlayerShapeshiftHookEvent>();

        public static readonly Hook<PlayerHookEvent> PlayerExiledHook = new SimpleHook<PlayerHookEvent>();
        public static readonly Hook<PlayerTeleportedHookEvent> PlayerTeleportedHook = new SimpleHook<PlayerTeleportedHookEvent>();
    }

    public static class ModHooks
    {
        public static readonly Hook<PlayerStatusReceivedHook> StatusReceivedHook = new SimpleHook<PlayerStatusReceivedHook>();
        public static readonly Hook<CustomCommandHookEvent> CustomCommandHook = new SimpleHook<CustomCommandHookEvent>();
    }

    public static class MeetingHooks
    {
        public static readonly Hook<MeetingHookEvent> MeetingCalledHook = new SimpleHook<MeetingHookEvent>();
        public static readonly Hook<CastVoteHookEvent> CastVoteHook = new SimpleHook<CastVoteHookEvent>();
        public static readonly Hook<ExiledHookEvent> ExiledHook = new SimpleHook<ExiledHookEvent>();
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