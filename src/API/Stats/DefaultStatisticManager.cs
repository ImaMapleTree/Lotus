using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Lotus.API.Reactive;
using Lotus.Managers;
using VentLib.Logging;
using VentLib.Options;
using VentLib.Utilities.Extensions;

namespace Lotus.API.Stats;

sealed class DefaultStatisticManager : Statistics
{
    private FileInfo? file;
    private const string CachePlayerStatsHookHey = nameof(CachePlayerStatsHookHey);
    private static readonly Dictionary<string, Statistic> BoundStatistics = new();
    private readonly Dictionary<string, Statistic> trackedStatistics = new(BoundStatistics);
    private static bool allowCaching;

    static DefaultStatisticManager()
    {
        OptionManager reportingOptionManager = OptionManager.GetManager(file: "file_options.txt");

        var cacheStatsOption = new OptionBuilder().Name("Cache Player Statistics")
            .Description("Allows caching of player statistics")
            .Value(true)
            .Value(false)
            .BuildAndRegister(reportingOptionManager);

        allowCaching = cacheStatsOption.GetValue<bool>();
    }

    public DefaultStatisticManager(string? fileName)
    {
        if (allowCaching) Hooks.GameStateHooks.GameEndHook.Bind(CachePlayerStatsHookHey, _ => CachePlayerStats(), true);
        file = fileName != null ? PluginDataManager.HiddenDataDirectory.GetFile(fileName) : null;
    }

    public void Track(Statistic statistic)
    {
        trackedStatistics[statistic.Identifier()] = statistic;
        if (statistic is not IPersistentStatistic) return;
        bool hasKey = BoundStatistics.ContainsKey(statistic.Identifier());
        BoundStatistics[statistic.Identifier()] = statistic;
        if (hasKey || file == null || file.Exists == false || statistic is not IJsonStats jsonStats) return;
        StatisticDump? dump = JsonSerializer.Deserialize<StatisticDump>(file.ReadAll());
        Dictionary<string, string>? jsonDict = dump?.Statistics.GetValueOrDefault(jsonStats.Identifier());
        if (jsonDict == null) return;
        jsonStats.FromJsonDict(jsonDict);
    }

    public List<Statistic> GetAllStats() => trackedStatistics.Values.ToList();

    public Statistic<T> GetStat<T>(string identifier) => (Statistic<T>)trackedStatistics[identifier];

    public T? GetValue<T>(UniquePlayerId playerId, string identifier) => GetStat<T>(identifier).GetValue(playerId);

    private void CachePlayerStats()
    {
        VentLogger.Info("Caching player stats");
        if (file == null) return;
        Dictionary<string, Dictionary<string, string>> jsonStatistics = new();
        trackedStatistics.ForEach(kv =>
        {
            if (kv.Value is not IJsonStats jsonStats) return;
            VentLogger.Info($"Stats: {jsonStats}");
            jsonStatistics[kv.Key] = jsonStats.ToJsonDict();
        });

        StatisticDump dump = new() { Statistics = jsonStatistics };
        string json = JsonSerializer.Serialize(dump);
        StreamWriter writer = file.OpenWriter(fileMode: FileMode.Create);
        writer.Write(json);
        writer.Close();
    }
}