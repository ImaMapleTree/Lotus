using System;
using Lotus.API.Odyssey;

namespace Lotus.Managers.History;

//[Localization(Group = "Hello")]
public abstract class HistoryEvent
{
    private DateTime timestamp = DateTime.Now;

    public abstract string CreateReport();

    public string RelativeTimestamp() => $"[{(timestamp - Game.MatchData.StartTime):mm\\:ss\\.ff]}";
}