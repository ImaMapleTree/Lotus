using System;
using TOHTOR.API;
using TOHTOR.API.Odyssey;

namespace TOHTOR.Managers.History;

//[Localization(Group = "Hello")]
public abstract class HistoryEvent
{
    private DateTime timestamp = DateTime.Now;

    public abstract string CreateReport();

    public string RelativeTimestamp() => $"[{(timestamp - Game.StartTime):mm\\:ss\\.ff]}";
}