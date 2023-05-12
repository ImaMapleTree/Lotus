using TOHTOR.API.Odyssey;

namespace TOHTOR.API.Reactive.HookEvents;

public class GameStateHookEvent : IHookEvent
{
    public MatchData MatchData;

    public GameStateHookEvent(MatchData matchData)
    {
        this.MatchData = matchData;
    }
}