using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.RPC;
using VentLib.Logging;
using VentLib.Utilities.Extensions;

// ReSharper disable ConvertIfStatementToSwitchStatement

namespace Lotus.Victory;

public static class VictoryScreen
{
    public static void ShowWinners(List<PlayerControl> winners, GameOverReason reason)
    {
        List<FrozenPlayer> winnerRoles = Game.MatchData.GameHistory.LastWinners = winners.Select(w => Game.MatchData.FrozenPlayers[w.GetGameID()]).ToList();
        VentLogger.Info($"Setting Up Win Screen | Winners: {winnerRoles.Select(fp => $"{fp.Name} ({fp.Role.EnglishRoleName})")}");

        bool impostorsWin = IsImpostorsWin(reason);

        List<PlayerControl> losers = Game.GetAllPlayers().ToList();

        winners.Do(winner =>
        {
            if (impostorsWin && !winner.Data.Role.IsImpostor) winner.CRpcSetRole(RoleTypes.ImpostorGhost);
            if (!impostorsWin && winner.Data.Role.IsImpostor) winner.CRpcSetRole(RoleTypes.CrewmateGhost);
            losers.RemoveAll(p => p.PlayerId == winner.PlayerId);
        });

        if (winners.Any(p => p.IsHost())) winners.Do(p => p.SetRole(impostorsWin ? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost));

        losers.Do(loser =>
        {
            if (impostorsWin && loser.Data.Role.IsImpostor) loser.CRpcSetRole(RoleTypes.CrewmateGhost);
            if (!impostorsWin && !loser.Data.Role.IsImpostor) loser.CRpcSetRole(RoleTypes.ImpostorGhost);
        });

        if (winners.Any(p => p.IsHost())) losers.Do(p => p.SetRole(impostorsWin ? RoleTypes.CrewmateGhost : RoleTypes.ImpostorGhost));
        Hooks.ResultHooks.WinnersHook.Propagate(new WinnersHookEvent(winnerRoles));
        Hooks.ResultHooks.LosersHook.Propagate(new LosersHookEvent(losers.Select(l => Game.MatchData.FrozenPlayers[l.GetGameID()]).ToList()));
    }

    private static bool IsImpostorsWin(GameOverReason reason)
    {
        return reason switch
        {
            GameOverReason.HumansByVote => false,
            GameOverReason.HumansByTask => false,
            GameOverReason.ImpostorByVote => true,
            GameOverReason.ImpostorByKill => true,
            GameOverReason.ImpostorBySabotage => true,
            GameOverReason.ImpostorDisconnect => false,
            GameOverReason.HumansDisconnect => true,
            GameOverReason.HideAndSeek_ByTimer => false,
            GameOverReason.HideAndSeek_ByKills => true,
            _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
        };
    }
}