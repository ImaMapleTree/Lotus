using Lotus.API.Odyssey;
using Lotus.GameModes;

namespace Lotus.API.Reactive.HookEvents;

public class GameStateHookEvent : IHookEvent
{
    public MatchData MatchData { get; }
    public IGameMode CurrentGameMode { get; }

    public GameStateHookEvent(MatchData matchData, IGameMode currentGameMode)
    {
        this.MatchData = matchData;
        this.CurrentGameMode = currentGameMode;
    }
}