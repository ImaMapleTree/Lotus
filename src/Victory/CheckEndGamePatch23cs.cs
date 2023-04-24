/*using System;
using System.Collections.Generic;
using HarmonyLib;
using TOHTOR.API;
using TOHTOR.Options;
using TOHTOR.Victory.Conditions;
using VentLib.Logging;
using VentLib.Utilities;

namespace TOHTOR.Victory;

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
public class CheckEndGamePatch23
{
    private static bool _deferred;
    private static DateTime slowDown = DateTime.Now;

    private static bool Prefix() => !AmongUsClient.Instance.AmHost;

    // Begin end game check on another thread
    static CheckEndGamePatch23()
    {
        Async.Schedule(Prefix2, 0.25f, true);
    }

    private static void Prefix2()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (Game.State is GameState.InLobby or GameState.InIntro) return;
        if (_deferred) return;

        //uint id = Profilers.Global.Sampler.Start("CheckEndGamePatch");

        WinDelegate winDelegate = Game.GetWinDelegate();
        if (StaticOptions.NoGameEnd)
            winDelegate.CancelGameWin();

        bool isGameWin = winDelegate.IsGameOver();
        if (!isGameWin)
        {
            //Profilers.Global.Sampler.Stop(id, "CheckEndGamePatch-NotGameWin");
            return;
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
        //Profilers.Global.Sampler.Stop(id);
    }


    private static void DelayedWin(GameOverReason reason)
    {
        _deferred = false;
        VentLogger.Info("Ending Game", "DelayedWin");
        GameManager.Instance.RpcEndGame(reason, false);
        Async.Schedule(() => GameManager.Instance.EndGame(), 0.1f);
    }
}*/