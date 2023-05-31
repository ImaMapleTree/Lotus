using System;
using System.Linq;

namespace Lotus.Managers.Reporting;

public enum ReportTag
{
    KickByAnticheat,
    KickByPacket,
    Performance
}

public static class ReportTags
{
    public static ReportTag[] Excluding(params ReportTag[] tags)
    {
        return Enum.GetValues<ReportTag>().Except(tags).ToArray();
    }
}