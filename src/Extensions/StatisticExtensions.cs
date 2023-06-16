using Lotus.API.Player;
using Lotus.API.Stats;

namespace Lotus.Extensions;

public static class StatisticExtensions
{
    public static void Increment(this IAccumulativeStatistic<int> statistic, UniquePlayerId uniquePlayerId)
    {
        statistic.Update(uniquePlayerId, i => i + 1);
    }
}