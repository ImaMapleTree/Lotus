using System;
using System.Collections.Generic;
using HarmonyLib;
using TOHTOR.API;
using TOHTOR.Options;
using TOHTOR.Victory.Conditions;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Debug.Profiling;

namespace TOHTOR.Victory;

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
public class CheckEndGamePatch2
{
    private static bool _deferred;
    private static DateTime slowDown = DateTime.Now;

    private static bool CodeToTest()
    {
        return true;
    }

    public static bool Prefix()
    {
        if (DateTime.Now.Subtract(slowDown).TotalSeconds < 0.1f) return false;
        slowDown = DateTime.Now;
        if (!AmongUsClient.Instance.AmHost) return true;
        if (_deferred) return false;

        uint id = Profilers.Global.Sampler.Start("CheckEndGamePatch");

        WinDelegate winDelegate = Game.GetWinDelegate();
        if (StaticOptions.NoGameEnd)
            winDelegate.CancelGameWin();

        bool isGameWin = winDelegate.IsGameOver();
        if (!isGameWin)
        {
            Profilers.Global.Sampler.Stop(id, "CheckEndGamePatch-NotGameWin");
            return false;
        }


        List<PlayerControl> winners = winDelegate.GetWinners();
        bool impostorsWon = winners.Count == 0 || winners[0].Data.Role.IsImpostor;

        GameOverReason reason = winDelegate.GetWinReason() switch
        {
            WinReason.FactionLastStanding => impostorsWon ? GameOverReason.ImpostorByKill : GameOverReason.HumansByVote,
            WinReason.RoleSpecificWin => impostorsWon ? GameOverReason.ImpostorByKill : GameOverReason.HumansByVote,
            WinReason.TasksComplete => GameOverReason.HumansByTask,
            WinReason.Sabotage => GameOverReason.ImpostorBySabotage,
            WinReason.NoWinCondition => GameOverReason.ImpostorDisconnect,
            WinReason.HostForceEnd => GameOverReason.ImpostorDisconnect,
            WinReason.GamemodeSpecificWin => GameOverReason.ImpostorByKill,
            WinReason.SoloWinner => GameOverReason.ImpostorByKill,
            _ => throw new ArgumentOutOfRangeException()
        };


        VictoryScreen.ShowWinners(winDelegate.GetWinners(), reason);

        _deferred = true;
        Async.Schedule(() => DelayedWin(reason), NetUtils.DeriveDelay(0.6f));

        Profilers.Global.Sampler.Stop(id);
        return false;
    }

    private static void DelayedWin(GameOverReason reason)
    {
        _deferred = false;
        VentLogger.Info("Ending Game", "DelayedWin");
        GameManager.Instance.RpcEndGame(reason, false);
        Async.Schedule(() => GameManager.Instance.EndGame(), 0.1f);
    }

    public static void ShowcaseFunction()
    {
        Profiler profiler = Profilers.Global; //デフォルトのプロファイラー。独自のプロファイラーを作成できます
        uint id = profiler.Sampler.Start(); // or profiler.Sampler.Start("NAME");

        bool condition = CodeToTest();

        if (!condition) profiler.Sampler.Discard(id); //.Discard は最後のピリオドの結果を無視します
        else profiler.Sampler.Stop(id); //記録期間
    }
}