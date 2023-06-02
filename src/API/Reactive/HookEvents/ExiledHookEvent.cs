using System.Collections.Generic;

namespace Lotus.API.Reactive.HookEvents;

public class ExiledHookEvent: IHookEvent
{
    public GameData.PlayerInfo ExiledPlayer;
    public List<byte> Voters;

    public ExiledHookEvent(GameData.PlayerInfo exiledPlayer, List<byte> voters)
    {
        ExiledPlayer = exiledPlayer;
        Voters = voters;
    }
}