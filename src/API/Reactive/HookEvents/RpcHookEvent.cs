using VentLib.Networking.RPC;

namespace Lotus.API.Reactive.HookEvents;

public struct RpcHookEvent : IHookEvent
{
    public RpcMeta Meta { get; }

    public RpcHookEvent(RpcMeta meta)
    {
        Meta = meta;
    }
}