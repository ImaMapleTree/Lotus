namespace Lotus.API.Vanilla;

public static class MatchState
{
    public static bool IsCountDown => GameStartManager.InstanceExists && GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown;
}