using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TOHTOR.API.Meetings;
using TOHTOR.Extensions;
using TOHTOR.Utilities;
using VentLib.Logging;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using static MeetingHud;

namespace TOHTOR.Patches.Meetings;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
public class CheckForEndVotingPatch
{
    public static bool Prefix(MeetingHud __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return true;
        MeetingDelegate meetingDelegate = MeetingDelegate.Instance;
        if (!meetingDelegate.IsForceEnd() && __instance.playerStates.Any(ps => !ps.AmDead && !ps.DidVote)) return false;
        VentLogger.Debug("Beginning End Voting", "CheckEndVotingPatch");
        List<VoterState> votingStates = new();
        meetingDelegate.CurrentVotes().ForEach(kv =>
        {
            byte playerId = kv.Key;
            Optional<PlayerControl> player = Utils.PlayerById(playerId);
            kv.Value.ForEach(voted =>
            {
                string votedName = voted.FlatMap(b => Utils.PlayerById(b)).Map(p => p.GetNameWithRole()).OrElse("No One");
                player.IfPresent(p => VentLogger.Log(LogLevel.All,$"{p.GetNameWithRole()} voted for {votedName}"));
                votingStates.Add(new VoterState
                {
                    VoterId = playerId,
                    VotedForId = voted.OrElse(253) // Skip vote byte
                });
            });
        });

        VentLogger.Trace($"End Vote Count: {meetingDelegate.CurrentVoteCount().Select(kv => $"{Utils.GetPlayerById(kv.Key).GetNameWithRole()}: {kv.Value}").Join()}");

        List<KeyValuePair<byte, int>> sortedVotes = meetingDelegate.CurrentVoteCount().Sorted(kvp => kvp.Value).Reverse().ToList();
        bool isTie = false;
        byte exiledPlayer = byte.MaxValue;
        switch (sortedVotes.Count)
        {
            case 0: break;
            case 1:
                exiledPlayer = sortedVotes[0].Key;
                break;
            case >= 2:
                isTie = sortedVotes[0].Value == sortedVotes[1].Value;
                exiledPlayer = sortedVotes[0].Key;
                break;
        }

        VentLogger.Trace($"Player With Most Votes: {Utils.PlayerById(exiledPlayer)}");

        GameData.PlayerInfo? playerInfo = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(info => !isTie && info.PlayerId == exiledPlayer);
        MeetingApi.EndVoting(__instance, votingStates.ToArray(), playerInfo, isTie);
        return false;
    }
}