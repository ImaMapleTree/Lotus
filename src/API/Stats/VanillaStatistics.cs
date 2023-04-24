using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using TOHTOR.API.Reactive;
using TOHTOR.API.Vanilla.Sabotages;
using TOHTOR.Extensions;
using VentLib.Localization.Attributes;
using VentLib.Utilities.Extensions;

namespace TOHTOR.API.Stats;

[Localized("Statistics")]
public class VanillaStatistics
{
    private static readonly Dictionary<byte, DateTime> DeathTimes = new();
    private static bool _firstMurder;
    private const string StatisticsHookKey = nameof(StatisticsHookKey);

    [Localized("Games")] private static string _games = "Games";
    [Localized("Wins")] private static string _wins = "Wins";
    [Localized("Losses")] private static string _losses = "Losses";
    [Localized("Kills")] private static string _playerKills = "Kills";
    [Localized("Deaths")] private static string _deaths = "Deaths";
    [Localized("FirstKills")] private static string _firstKills = "First Kills";
    [Localized("FirstDeaths")] private static string _firstDeaths = "First Deaths";
    [Localized("Shapeshifts")] private static string _shapeshifts = "Shapeshifts";
    [Localized("TimesVented")] private static string _timesVented = "Times Venting";
    [Localized("TotalTasksComplete")] private static string _totalTasksComplete = "Tasks Completed";
    [Localized("ShortTasksComplete")] private static string _shortTasksComplete = "Short Tasks Completed";
    [Localized("LongTasksComplete")] private static string _longTasksComplte = "Long Tasks Completed";
    [Localized("CommonTasksComplete")] private static string _commonTasksComplete = "Common Tasks Completed";
    [Localized("LightsSabotaged")] private static string _lightsSabotaged = "Lights Sabotaged";
    [Localized("ReactorSabotaged")] private static string _reactorSabotaged = "Reactor Sabotaged";
    [Localized("OxygenSabotaged")] private static string _oxygenSabotaged = "Oxygen Sabotaged";
    [Localized("CommsSabotaged")] private static string _commsSabotaged = "Comms Sabotaged";
    [Localized("DoorsSabotaged")] private static string _doorsSabotaged = "Doors Sabotaged";
    [Localized("SabotagesCalled")] private static string _sabotagesCalled = "Sabotages Called";
    [Localized("SabotagesFixed")] private static string _sabotagesFixed = "Sabotages Fixed";
    [Localized("MeetingsCalled")] private static string _meetingsCalled = "Meetings Called";
    [Localized("BodiesReported")] private static string _bodiesReported = "Bodies Reported";
    [Localized("MessagesSent")] private static string _messagesSent = "Messages Sent";
    [Localized("PlayersExiled")] private static string _playersExiled = "Players Exiled";
    [Localized("TimesExiled")] private static string _timesExiled = "Times Exiled";
    [Localized("TimesVoteSkipped")] private static string _timesVoteSkipped = "Votes Skipped";
    [Localized("TimesVoted")] private static string _timesVoted = "Times Voted";
    [Localized("TotalVotesFor")] private static string _timesVotedFor = "Total Votes for Player";
    [Localized("TimeAlive")] private static string _timesAlive = "Time Spent Alive";
    [Localized("TimeDead")] private static string _timeDead = "Time Spent Dead";
    [Localized("TotalTime")] private static string _totalTime = "Total Play Time";

    public static IAccumulativeStatistic<int> Games = Statistic<int>.CreateAccumulative(Identifiers.Games, _games);
    public static IAccumulativeStatistic<int> Wins = Statistic<int>.CreateAccumulative(Identifiers.Wins, _wins);
    public static IAccumulativeStatistic<int> Losses = Statistic<int>.CreateAccumulative(Identifiers.Losses, _losses);
    public static IAccumulativeStatistic<int> Kills = Statistic<int>.CreateAccumulative(Identifiers.Kills, _playerKills);
    public static IAccumulativeStatistic<int> Deaths = Statistic<int>.CreateAccumulative(Identifiers.Deaths, _deaths);
    public static IAccumulativeStatistic<int> FirstKills = Statistic<int>.CreateAccumulative(Identifiers.FirstKills, _firstKills);
    public static IAccumulativeStatistic<int> FirstDeaths = Statistic<int>.CreateAccumulative(Identifiers.FirstDeaths, _firstDeaths);
    public static IAccumulativeStatistic<int> Shapeshifts = Statistic<int>.CreateAccumulative(Identifiers.Shapeshifts, _shapeshifts);
    public static IAccumulativeStatistic<int> TimesVented = Statistic<int>.CreateAccumulative(Identifiers.TimesVented, _timesVented);

    public static IAccumulativeStatistic<int> TasksComplete = Statistic<int>.CreateAccumulative(Identifiers.TasksComplete, _totalTasksComplete);
    public static IAccumulativeStatistic<int> ShortTasksComplete = Statistic<int>.CreateAccumulative(Identifiers.ShortTasksComplete, _shortTasksComplete);
    public static IAccumulativeStatistic<int> LongTasksComplete = Statistic<int>.CreateAccumulative(Identifiers.LongTasksComplete, _longTasksComplte);
    public static IAccumulativeStatistic<int> CommonTasksComplete = Statistic<int>.CreateAccumulative(Identifiers.CommonTasksComplete, _commonTasksComplete);

    public static IAccumulativeStatistic<int> LightsSabotaged = Statistic<int>.CreateAccumulative(Identifiers.LightsSabotaged, _lightsSabotaged);
    public static IAccumulativeStatistic<int> ReactorSabotaged = Statistic<int>.CreateAccumulative(Identifiers.ReactorSabotaged, _reactorSabotaged);
    public static IAccumulativeStatistic<int> OxygenSabotaged = Statistic<int>.CreateAccumulative(Identifiers.OxygenSabotaged, _oxygenSabotaged);
    public static IAccumulativeStatistic<int> CommsSabotaged = Statistic<int>.CreateAccumulative(Identifiers.CommsSabotaged, _commsSabotaged);
    public static IAccumulativeStatistic<int> DoorsSabotaged = Statistic<int>.CreateAccumulative(Identifiers.DoorsSabotaged, _doorsSabotaged);
    public static IAccumulativeStatistic<int> SabotagesCalled = Statistic<int>.CreateAccumulative(Identifiers.SabotagesCalled, _sabotagesCalled);
    public static IAccumulativeStatistic<int> SabotagesFixed = Statistic<int>.CreateAccumulative(Identifiers.SabotagesFixed, _sabotagesFixed);

    public static IAccumulativeStatistic<int> MeetingsCalled = Statistic<int>.CreateAccumulative(Identifiers.MeetingsCalled, _meetingsCalled);
    public static IAccumulativeStatistic<int> BodiesReported = Statistic<int>.CreateAccumulative(Identifiers.BodiesReported, _bodiesReported);
    public static IAccumulativeStatistic<int> MessagesSent = Statistic<int>.CreateAccumulative(Identifiers.MessagesSent, _messagesSent);
    public static IAccumulativeStatistic<int> PlayersExiled = Statistic<int>.CreateAccumulative(Identifiers.PlayersExiled, _playersExiled);
    public static IAccumulativeStatistic<int> TimesExiled = Statistic<int>.CreateAccumulative(Identifiers.TimesExiled, _timesExiled);
    public static IAccumulativeStatistic<int> TimesVoteSkipped = Statistic<int>.CreateAccumulative(Identifiers.TimesVoteSkipped, _timesVoteSkipped);
    public static IAccumulativeStatistic<int> TimesVoted = Statistic<int>.CreateAccumulative(Identifiers.TimesVoted, _timesVoted);
    public static IAccumulativeStatistic<int> TotalVotesFor = Statistic<int>.CreateAccumulative(Identifiers.TotalVotesFor, _timesVotedFor);

    public static IAccumulativeStatistic<double> TimeAlive = Statistic<double>.CreateAccumulative(Identifiers.TimeAlive, _timesAlive);
    public static IAccumulativeStatistic<double> TimeDead = Statistic<double>.CreateAccumulative(Identifiers.TimeDead, _timeDead);
    public static IAccumulativeStatistic<double> TotalTime = Statistic<double>.CreateAccumulative(Identifiers.TotalTime, _totalTime);

    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
    public static class Identifiers
    {
        public const string Games = "Vanilla.Player.Games";
        public const string Wins = "Vanilla.Player.Wins";
        public const string Losses = "Vanilla.Player.Losses";
        public const string Kills = "Vanilla.Player.Kills";
        public const string Deaths = "Vanilla.Player.Deaths";
        public const string FirstKills = "Vanilla.Player.FirstKills";
        public const string FirstDeaths = "Vanilla.Player.FirstDeaths";
        public const string Shapeshifts = "Vanilla.Player.Shapeshifts";
        public const string TimesVented = "Vanilla.Player.TimesVented";
        public const string TasksComplete = "Vanilla.Player.TasksComplete";
        public const string ShortTasksComplete = "Vanilla.Player.ShortTasksComplete";
        public const string LongTasksComplete = "Vanilla.Player.LongTasksComplete";
        public const string CommonTasksComplete = "Vanilla.Player.CommonTasksComplete";
        public const string LightsSabotaged = "Vanilla.Player.LightsSabotaged";
        public const string ReactorSabotaged = "Vanilla.Player.ReactorSabotaged";
        public const string OxygenSabotaged = "Vanilla.Player.OxygenSabotaged";
        public const string CommsSabotaged = "Vanilla.Player.CommsSabotaged";
        public const string DoorsSabotaged = "Vanilla.Player.DoorsSabotaged";
        public const string SabotagesCalled = "Vanilla.Player.SabotagesCalled";
        public const string SabotagesFixed = "Vanilla.Player.SabotagesFixed";
        public const string MeetingsCalled = "Vanilla.Player.MeetingsCalled";
        public const string BodiesReported = "Vanilla.Player.BodiesReported";
        public const string MessagesSent = "Vanilla.Player.MessagesSent";
        public const string PlayersExiled = "Vanilla.Player.PlayersExiled";
        public const string TimesExiled = "Vanilla.Player.TimesExiled";
        public const string TimesVoteSkipped = "Vanilla.Player.TimesVoteSkipped";
        public const string TimesVoted = "Vanilla.Player.TimesVoted";
        public const string TotalVotesFor = "Vanilla.Player.TotalVotesFor";
        public const string TimeAlive = "Vanilla.Player.TimeAlive";
        public const string TimeDead = "Vanilla.Player.TimeDead";
        public const string TotalTime = "Vanilla.Player.TotalTime";
    }

    static VanillaStatistics()
    {
        SetupStatisticHooks();
        SetupStatisticTracking();
    }


    private static void SetupStatisticHooks()
    {
        Hooks.GameStateHooks.GameStartHook.Bind(StatisticsHookKey, _ =>
        {
            _firstMurder = true;
            DeathTimes.Clear();
        });
        Hooks.GameStateHooks.GameEndHook.Bind(StatisticsHookKey, _ =>
        {
            Game.GetAllPlayers().ForEach(p => Games.Update(p.PlayerId, i => i + 1));
            Game.GetAllPlayers().ForEach(p => TotalTime.Update(p.PlayerId, DateTime.Now.Subtract(Game.StartTime).TotalSeconds));
            HashSet<byte> allPlayerIds = Game.GetAllPlayers().Select(p => p.PlayerId).ToHashSet();
            allPlayerIds.Except(DeathTimes.Keys).ForEach(ap => TimeAlive.Update(ap, DateTime.Now.Subtract(Game.StartTime).TotalSeconds));
            DeathTimes.ForEach(dt => TimeDead.Update(dt.Key, DateTime.Now.Subtract(dt.Value).TotalSeconds));
        });
        Hooks.PlayerHooks.PlayerMurderHook.Bind(StatisticsHookKey, murderEvent =>
        {
            Kills.Update(murderEvent.Killer.UniquePlayerId(), i => i + 1);
            Deaths.Update(murderEvent.Victim.UniquePlayerId(), i => i + 1);
            TimeAlive.Update(murderEvent.Victim.UniquePlayerId(), DateTime.Now.Subtract(Game.StartTime).TotalSeconds);
            DeathTimes[murderEvent.Victim.PlayerId] = DateTime.Now;
            if (!_firstMurder) return;
            FirstKills.Update(murderEvent.Killer.UniquePlayerId(), i => i + 1);
            FirstDeaths.Update(murderEvent.Victim.UniquePlayerId(), i => i + 1);
            _firstMurder = false;
        });
        Hooks.MeetingHooks.MeetingCalledHook.Bind(StatisticsHookKey, meetingEvent =>
        {
            GameData.PlayerInfo? reported = meetingEvent.Reported;
            if (reported == null) MeetingsCalled.Update(meetingEvent.Caller.UniquePlayerId(), i => i + 1);
            else BodiesReported.Update(meetingEvent.Caller.UniquePlayerId(), i => i + 1);
        });
        Hooks.PlayerHooks.PlayerMessageHook.Bind(StatisticsHookKey, messageEvent => MessagesSent.Update(messageEvent.Player.UniquePlayerId(), i => i + 1));
        Hooks.SabotageHooks.SabotageCalledHook.Bind(StatisticsHookKey, sabotageEvent =>
        {
            SabotageType type = sabotageEvent.Sabotage.SabotageType();
            sabotageEvent.Sabotage.Caller().IfPresent(caller =>
            {
                switch (type)
                {
                    case SabotageType.Lights:
                        LightsSabotaged.Update(caller.PlayerId, i => i  + 1);
                        break;
                    case SabotageType.Reactor:
                        ReactorSabotaged.Update(caller.PlayerId, i => i + 1);
                        break;
                    case SabotageType.Communications:
                        CommsSabotaged.Update(caller.PlayerId, i => i + 1);
                        break;
                    case SabotageType.Oxygen:
                        OxygenSabotaged.Update(caller.PlayerId, i => i + 1);
                        break;
                    case SabotageType.Door:
                        DoorsSabotaged.Update(caller.PlayerId, i => i + 1);
                        break;
                    case SabotageType.Helicopter:
                        ReactorSabotaged.Update(caller.PlayerId, i => i + 1);
                        break;
                }

                SabotagesCalled.Update(caller.PlayerId, i => i + 1);
            });
        });
        Hooks.SabotageHooks.SabotageFixedHook.Bind(StatisticsHookKey, sabotageFixEvent =>
        {
            sabotageFixEvent.Fixer.IfPresent(fixer => SabotagesFixed.Update(fixer.UniquePlayerId(), i => i + 1));
        });
    }

    private static void SetupStatisticTracking()
    {
        Statistics statistics = Statistics.Current();
        typeof(VanillaStatistics).GetFields(BindingFlags.Static | BindingFlags.Public)
            .Select(field => (Statistic)field.GetValue(null)!)
            .ForEach(stat => statistics.Track(stat));
    }
}