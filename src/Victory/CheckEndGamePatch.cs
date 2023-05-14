using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.Managers.History;
using Lotus.Managers.Hotkeys;
using Lotus.Options;
using Lotus.Utilities;
using Lotus.Victory.Conditions;
using UnityEngine;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Debug.Profiling;

namespace Lotus.Victory;

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
public class CheckEndGamePatch
{
    public static bool Deferred;
    private static DateTime slowDown = DateTime.Now;
    private static FixedUpdateLock _fixedUpdateLock = new FixedUpdateLock(0.1f);

    static CheckEndGamePatch()
    {
        HotkeyManager.Bind(KeyCode.LeftShift, KeyCode.L, KeyCode.Return)
            .If(p => p.HostOnly().State(Game.IgnStates))
            .Do(() =>
            {
                Deferred = false;
                ManualWin manualWin = new(new List<PlayerControl>(), WinReason.HostForceEnd);
                manualWin.Activate();
                GameManager.Instance.LogicFlow.CheckEndCriteria();
            });
    }

    public static bool Prefix()
    {
        if (!AmongUsClient.Instance.AmHost) return true;
        if (Game.State is GameState.InLobby or GameState.InIntro) return false;
        if (!_fixedUpdateLock.AcquireLock()) return false;
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

        
        Game.MatchData.GameHistory.PlayerHistory = Game.MatchData.FrozenPlayers.Values.Select(p => new PlayerHistory(p)).ToList();
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