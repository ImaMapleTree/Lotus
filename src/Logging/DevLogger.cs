using System;
using System.Linq;
using Lotus.Utilities;
using VentLib.Logging;
using VentLib.Utilities.Extensions;

namespace Lotus.Logging;

public static class DevLogger
{
    private static bool _enabled;

    static DevLogger()
    {
        #if DEBUG
        _enabled = true;
        #endif
    }

    private static LogLevel LogLevel = LogLevel.Fatal.Similar("DEV", ConsoleColor.Cyan);
    private static LogLevel LogLevelLow = LogLevel.Trace.Similar("DEVLOW", ConsoleColor.Gray);

    public static void Low(string message)
    {
        if (_enabled) VentLogger.Log(LogLevelLow, message, "DEVLOW");
    }

    public static void Log(string message)
    {
        if (_enabled) VentLogger.Log(LogLevel, message, "DEV");
    }

    public static void Log(object obj)
    {
        if (_enabled) VentLogger.Log(LogLevel, obj.ToString()!, "DEV");
    }

    public static void GameInfo()
    {
        if (_enabled)
            VentLogger.Log(LogLevel, GameData.Instance.AllPlayers.ToArray().Select(p => (p.PlayerName.RemoveHtmlTags().Replace("\n", ""), p.Role.Role, "Dead " + p.IsDead, "Disconnected " + p.Disconnected)).StrJoin(), "DEV");
    }
}