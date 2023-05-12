using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using TOHTOR.Extensions;
using TOHTOR.Managers;
using VentLib.Logging;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using static MeetingHud;

namespace TOHTOR.API.Vanilla.Meetings;

public class MeetingDelegate
{
    public static MeetingDelegate Instance = null!;
    public GameData.PlayerInfo? ExiledPlayer { get; set; }
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

    public void AddVote(PlayerControl player, Optional<PlayerControl> target)
    {
        VentLogger.Trace($"{player.GetNameWithRole()} casted vote for {target.Map(p => p.GetNameWithRole()).OrElse("No One")}");
        AddVote(player.PlayerId, target.Map(p => p.PlayerId));
    }

    public void AddVote(byte playerId, Optional<byte> target)
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
        MeetingHud.RpcVotingComplete(voterStates, exiledPlayer, isTie);
    }

    public void EndVoting(GameData.PlayerInfo? exiledPlayer, bool isTie = false)
    {
        MeetingHud.RpcVotingComplete(new Il2CppStructArray<VoterState>(0), exiledPlayer, isTie);
    }

    internal bool IsForceEnd() => isForceEnd;
}