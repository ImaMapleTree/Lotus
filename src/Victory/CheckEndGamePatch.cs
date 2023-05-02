using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TOHTOR.API.Odyssey;
using TOHTOR.Managers.History;
using TOHTOR.Options;
using TOHTOR.Victory.Conditions;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Debug.Profiling;

namespace TOHTOR.Victory;

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
public class CheckEndGamePatch
{
    public static bool Deferred;
    private static DateTime slowDown = DateTime.Now;

    public static bool Prefix()
    {
        if (Game.State is GameState.InLobby or GameState.InIntro) return false;
        if (DateTime.Now.Subtract(slowDown).TotalSeconds < 0.1f) return false;
        slowDown = DateTime.Now;
        if (!AmongUsClient.Instance.AmHost) return true;
        if (Deferred) return false;

        uint id = Profilers.Global.Sampler.Start("CheckEndGamePatch");

        WinDelegate winDelegate = Game.GetWinDelegate();
        if (GeneralOptions.DebugOptions.NoGameEnd) winDelegate.CancelGameWin();

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
        };


        Game.GameHistory.PlayerHistory = Game.GameHistory.FrozenPlayers.Values.Select(p => new PlayerHistory(p)).ToList();
        VictoryScreen.ShowWinners(winDelegate.GetWinners(), reason);

        Deferred = true;
        Async.Schedule(() => DelayedWin(reason), NetUtils.DeriveDelay(1f));

        Profilers.Global.Sampler.Stop(id);
        return false;
    }

    private static void DelayedWin(GameOverReason reason)
    {
        Deferred = false;
        VentLogger.Info("Ending Game", "DelayedWin");
        GameManager.Instance.RpcEndGame(reason, false);
        Async.Schedule(() => GameManager.Instance.EndGame(), 0.1f);
    }
}