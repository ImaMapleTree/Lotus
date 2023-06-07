using System;
using VentLib.Logging;

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
}