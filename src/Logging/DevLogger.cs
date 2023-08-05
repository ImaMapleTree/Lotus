using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VentLib.Logging.Default;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VLogger = VentLib.Logging.Logger;

namespace Lotus.Logging;

public static class DevLogger
{
    private static bool _enabled;
    private static Dictionary<Type, VLogger> loggers = new();

    static DevLogger()
    {
        #if DEBUG
        _enabled = true;
        #endif
    }

    private static readonly LogLevel LogLevel = LogLevel.Fatal.Similar("DEV", ConsoleColor.Cyan);
    private static readonly LogLevel LogLevelLow = LogLevel.Trace.Similar("DEVLOW", ConsoleColor.Gray);

    public static void Low(string message)
    {
        MethodBase? callerMethod = Mirror.GetCaller();
        VLogger log = GetLogger(callerMethod?.DeclaringType ?? typeof(DevLogger));

        if (_enabled) log.Log(LogLevelLow, message, LogArguments.Wrap(log, Array.Empty<object?>(), null, callerMethod));
    }

    public static void Log(string message)
    {
        MethodBase? callerMethod = Mirror.GetCaller();
        VLogger log = GetLogger(callerMethod?.DeclaringType ?? typeof(DevLogger));

        if (_enabled) log.Log(LogLevel, message, LogArguments.Wrap(log, Array.Empty<object?>(), null, callerMethod));
    }

    public static void Log(object obj)
    {
        MethodBase? callerMethod = Mirror.GetCaller();
        VLogger log = GetLogger(callerMethod?.DeclaringType ?? typeof(DevLogger));

        if (_enabled) log.Log(LogLevel, obj.ToString()!, LogArguments.Wrap(log, Array.Empty<object?>(), null, callerMethod));
    }

    public static void GameInfo()
    {
        MethodBase? callerMethod = Mirror.GetCaller();
        VLogger log = GetLogger(callerMethod?.DeclaringType ?? typeof(DevLogger));
        if (_enabled)
            log.Log(LogLevel, GameData.Instance.AllPlayers.ToArray().Select(p => (p.PlayerName.Replace("\n", ""), p.Role.Role, "Dead " + p.IsDead, "Disconnected " + p.Disconnected)).StrJoin(), LogArguments.Wrap(log, Array.Empty<object?>(), null, callerMethod));
    }

    private static VLogger GetLogger(Type type) {
        return loggers.GetOrCompute(type, () =>
        {
            VLogger logger = LoggerFactory.GetLogger(type);
            if (logger is DefaultLogger defaultLogger) defaultLogger.Accumulators.Add(new DevLoggerColorer());
            return logger;
        });
    }

    private class DevLoggerColorer : ILogAccumulator
    {
        public LogComposite Accumulate(LogComposite composite, LogArguments arguments)
        {
            composite.Color = composite.Level.Name switch
            {
                "DEV" => LogLevel.Color,
                "DEVLOW" => LogLevelLow.Color,
                _ => composite.Color
            };
            return composite;
        }
    }
}