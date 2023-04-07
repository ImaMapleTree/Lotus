using System;
using System.Linq;

namespace TOHTOR.Managers.Reporting;

public enum ReportTag
{
    KickByAnticheat,
    Performance
}

public static class ReportTags
{
    public static ReportTag[] Excluding(params ReportTag[] tags)
    {
        return Enum.GetValues<ReportTag>().Except(tags).ToArray();
    }
}