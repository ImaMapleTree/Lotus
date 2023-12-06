namespace Lotus.Victory.Conditions;

public enum ReasonType
{
    NoWinCondition,
    HostForceEnd,
    FactionLastStanding,
    TasksComplete,
    GameModeSpecificWin,
    Sabotage,
    RoleSpecificWin,
    /// <summary>
    /// This reason should be used when only the players marked by the initial condition should win (prevents things like Survivor from winning)
    /// </summary>
    SoloWinner
}

public struct WinReason
{
    public ReasonType ReasonType;
    public string? ReasonText;

    public WinReason(ReasonType reasonType, string? reasonText = null)
    {
        ReasonType = reasonType;
        ReasonText = reasonText;
    }
}