using System;
using System.Globalization;
using TOHTOR.API;
using TOHTOR.API.Odyssey;

namespace TOHTOR.Managers.History;

/// <summary>
/// Simple class that stores its creation time
/// </summary>
public class Timestamp
{
    private TimeSpan time;

    public Timestamp()
    {
        time = DateTime.Now.Subtract(Game.MatchData.StartTime);
    }

    public bool IsBefore(Timestamp other) => time.CompareTo(other.time) < 0;

    public bool IsAfter(Timestamp other) => time.CompareTo(other.time) > 0;

    public override string ToString() => time.ToString();

    public string ToString(string formatter) => time.ToString(formatter, new CultureInfo("en-US"));
}