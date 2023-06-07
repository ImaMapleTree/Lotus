using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Player;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Managers;
using Lotus.Extensions;
using VentLib.Logging;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using static MeetingHud;

namespace Lotus.API.Vanilla.Meetings;

public class MeetingDelegate
{
    public static MeetingDelegate Instance = null!;
    public GameData.PlayerInfo? ExiledPlayer { get; set; }
    public HashSet<byte> TiedPlayers = new();

    public bool IsTie { get; set; }
    internal BlackscreenResolver BlackscreenResolver { get; }


    private MeetingHud MeetingHud => MeetingHud.Instance;
    private Dictionary<byte, List<Optional<byte>>> currentVotes = new();
    private bool isForceEnd;

    public MeetingDelegate()
    {
        Instance = this;
        BlackscreenResolver = new BlackscreenResolver(this);
    }

    public void CastVote(PlayerControl player, Optional<PlayerControl> target)
    {
        VentLogger.Trace($"{player.GetNameWithRole()} casted vote for {target.Map(p => p.GetNameWithRole()).OrElse("No One")}");
        CastVote(player.PlayerId, target.Map(p => p.PlayerId));
    }

    public void CastVote(byte playerId, Optional<byte> target)
    {
        currentVotes.GetOrCompute(playerId, () => new List<Optional<byte>>()).Add(target);
    }

    public void RemoveVote(PlayerControl player, Optional<PlayerControl> target) => RemoveVote(player.PlayerId, target.Map(p => p.PlayerId));

    public void RemoveVote(byte playerId, Optional<byte> target)
    {
        List<Optional<byte>> votes = currentVotes.GetOrCompute(playerId, () => new List<Optional<byte>>());
        int index = target.Transform(
            tId => votes.FindIndex(opt => opt.Map(b => b == tId).OrElse(false)),
            () => votes.Count - 1);
        if (index == -1) return;
        votes.RemoveAt(index);
    }

    public Dictionary<byte, int> CurrentVoteCount()
    {
        Dictionary<byte, int> counts = new() { { 255, 0 } };
        currentVotes.ForEach(kv =>
            kv.Value.Select(o => o.OrElse(255))
                .ForEach(b => counts[b] = counts.GetValueOrDefault(b, 0) + 1)
            );
        return counts;
    }

    public Dictionary<byte, List<Optional<byte>>> CurrentVotes() => currentVotes;

    public void EndVoting() => isForceEnd = true;

    public void EndVoting(Dictionary<byte, int> voteCounts, GameData.PlayerInfo? exiledPlayer, bool isTie = false)
    {
        List<VoterState> voterStates = new List<VoterState>();
        voteCounts.ForEach(t =>
        {
            VoterState voterState = new() { VotedForId = t.Key };
            for (int i = 0; i < t.Value; i++) voterStates.Add(voterState);
        });

        MeetingHud.RpcVotingComplete(voterStates.ToArray(), exiledPlayer, isTie);
    }

    public void EndVoting(VoterState[] voterStates, GameData.PlayerInfo? exiledPlayer, bool isTie = false)
    {
        this.ExiledPlayer = exiledPlayer;
        this.IsTie = isTie;

        if (ExiledPlayer != null)
        {
            List<byte> playerVotes = CurrentVotes().SelectMany(kv => kv.Value.Filter().Where(i => i == ExiledPlayer.PlayerId).Select(i => kv.Key)).ToList();
            PlayerControl p;
            if ((p = ExiledPlayer.Object) != null) p.SetName(AUSettings.ConfirmImpostor() ? $"<b>{p.GetCustomRole().RoleName}</b>\n{p.name}" : p.name);
            Hooks.MeetingHooks.ExiledHook.Propagate(new ExiledHookEvent(ExiledPlayer, playerVotes));
        }

        MeetingHud.RpcVotingComplete(voterStates, exiledPlayer, isTie);
    }

    public void EndVoting(GameData.PlayerInfo? exiledPlayer, bool isTie = false) => EndVoting(Array.Empty<VoterState>(), exiledPlayer, isTie);

    internal bool IsForceEnd() => isForceEnd;

    public void CalculateExiledPlayer()
    {
        List<KeyValuePair<byte, int>> sortedVotes = this.CurrentVoteCount().Sorted(kvp => kvp.Value).Reverse().ToList();
        bool isTie = false;
        byte exiledPlayer = byte.MaxValue;
        int mostVotes = 0;
        switch (sortedVotes.Count)
        {
            case 0: break;
            case 1:
                mostVotes = sortedVotes[0].Value;
                exiledPlayer = sortedVotes[0].Key;
                break;
            case >= 2:
                mostVotes = sortedVotes[0].Value;
                isTie = sortedVotes[0].Value == sortedVotes[1].Value;
                exiledPlayer = sortedVotes[0].Key;
                break;
        }

        TiedPlayers = sortedVotes.Where(sv => sv.Value == mostVotes).Select(sv => sv.Key).ToHashSet();

        this.ExiledPlayer = Players.PlayerById(exiledPlayer).Map(p => p.Data).OrElse(null!);
        this.IsTie = isTie;

        string mostVotedPlayer = this.ExiledPlayer?.Object != null ? this.ExiledPlayer.Object.name : "Unknown";
        VentLogger.Trace($"Calculated player votes. Player with most votes = {mostVotedPlayer}, isTie = {isTie}");

        if (IsTie) this.ExiledPlayer = null;
    }
}