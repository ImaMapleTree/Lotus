using InnerNet;

namespace Lotus.API.Reactive.HookEvents;

public class ClientConnectHookEvent: IHookEvent
{
    public ClientData Client;

    public ClientConnectHookEvent(ClientData client)
    {
        Client = client;
    }
}