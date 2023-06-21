namespace Lotus.API.Reactive.HookEvents;

public class GameJoinHookEvent: IHookEvent
{
    public bool IsNewLobby;
    public bool Loaded;

    public GameJoinHookEvent(bool isNewLobby)
    {
        IsNewLobby = isNewLobby;
    }
}