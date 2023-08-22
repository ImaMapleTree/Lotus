using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Extensions;
using Lotus.Logging;
using Lotus.RPC;
using VentLib.Networking.RPC;
using VentLib.Utilities.Extensions;

// ReSharper disable ConvertIfStatementToSwitchStatement

namespace Lotus.Victory;

public static class VictoryScreen
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(VictoryScreen));

    public static void ShowWinners(WinDelegate winDelegate, GameOverReason reason)
    {
        List<PlayerControl> winners = winDelegate.GetWinners();
        HashSet<byte> winningPlayerIds = winners.Select(p => p.PlayerId).ToHashSet();
        List<FrozenPlayer> winnerRoles = Game.MatchData.GameHistory.LastWinners = winners.Select(w => Game.MatchData.FrozenPlayers[w.GetGameID()]).Distinct().ToList();
        Game.MatchData.GameHistory.AdditionalWinners = winDelegate.GetAdditionalWinners().Select(w => Game.MatchData.FrozenPlayers[w.GetGameID()]).Distinct().ToList();
        log.Info($"Setting Up Win Screen | Winners: {winnerRoles.Select(fp => $"{fp.Name} ({fp.Role.EnglishRoleName})").Fuse()}");

        bool impostorsWin = IsImpostorsWin(reason);

        Players.GetPlayers().ForEach(p =>
        {
            bool wasAlive = p.IsAlive();
            RoleTypes roleType = winningPlayerIds.Contains(p.PlayerId) ^ !impostorsWin ? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost;
            DevLogger.Log($"Setting PLayer: {p.name} => {roleType}");
            p.CRpcSetRole(roleType);
            p.SetRole(roleType);
            p.Data.PlayerName = p.name;
            p.Data.IsDead = !wasAlive;
        });

        IEnumerable<PlayerControl> losers = Players.GetPlayers().Where(p => !winningPlayerIds.Contains(p.PlayerId));

        GeneralRPC.SendGameData();

        Hooks.ResultHooks.WinnersHook.Propagate(new WinnersHookEvent(winnerRoles));
        Hooks.ResultHooks.LosersHook.Propagate(new LosersHookEvent(losers.Select(l => Game.MatchData.FrozenPlayers[l.GetGameID()]).ToList()));
        GeneralRPC.SendGameData();
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