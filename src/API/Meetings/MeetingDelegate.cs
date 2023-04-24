using System.Collections.Generic;
using System.Linq;
using TOHTOR.Extensions;
using VentLib.Logging;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace TOHTOR.API.Meetings;

public class MeetingDelegate
{
    public static MeetingDelegate Instance = null!;

    private Dictionary<byte, List<Optional<byte>>> currentVotes = new();
    private bool isForceEnd;

    public MeetingDelegate()
    {
        Instance = this;
    }

    public void AddVote(PlayerControl player, Optional<PlayerControl> target)
    {
        VentLogger.Trace($"{player.GetNameWithRole()} casted vote for {target.Map(p => p.GetNameWithRole()).OrElse("No One")}");
        currentVotes.GetOrCompute(player.PlayerId, () => new List<Optional<byte>>()).Add(target.Map(p => p.PlayerId));
    }

    public void RemoveVote(PlayerControl player, Optional<PlayerControl> target)
    {
        List<Optional<byte>> votes = currentVotes.GetOrCompute(player.PlayerId, () => new List<Optional<byte>>());
        int index = target.Map(p => p.PlayerId).Transform(
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

    internal bool IsForceEnd() => isForceEnd;
}