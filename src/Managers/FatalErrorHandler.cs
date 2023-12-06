using System;
using System.Collections.Generic;
using Lotus.API.Reactive;
using Lotus.Chat;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers;

[Localized($"Internals.{nameof(FatalErrorHandler)}")]
public class FatalErrorHandler
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(FatalErrorHandler));
    private static readonly Queue<(Exception, string)> ExceptionMessages = new();

    [Localized("FatalErrorTitle")]
    private static string fatalErrorTitle = "Fatal Error";

    [Localized("FatalErrorMessage")]
    private static string fatalErrorMessage = "Encountered fatal error during [{0}]";

    [Localized("CheckLogMessage")]
    private static string checkLogMessage = "Check logs for more info.";

    static FatalErrorHandler()
    {
        Hooks.NetworkHooks.GameJoinHook.Bind(typeof(FatalErrorHandler), ReportErrors);
    }

    public static void ForceEnd(Exception exception, string phase)
    {
        if (GameManager.Instance == null) return;
        log.Exception(exception);
        ExceptionMessages.Enqueue((exception, phase));
        Async.Schedule(() => GameManager.Instance.RpcEndGame(GameOverReason.ImpostorDisconnect, false), 1);
    }

    private static void ReportErrors()
    {
        if (ExceptionMessages.Count == 0) return;
        log.Fatal("FORCED ENDED GAME DUE TO FATAL ERRORS. ERRORS LISTED BELOW");
        while (ExceptionMessages.TryDequeue(out (Exception ex, string phase) tuple))
        {
            string msg = fatalErrorMessage.Formatted(tuple.phase);
            log.Exception(msg, tuple.ex);
            ChatHandler.Of(msg + " " + checkLogMessage, Color.red.Colorize(fatalErrorTitle)).Send();
        }
        log.Fatal("========================== [ END OF FATAL ERRORS ] ==========================");
    }
}