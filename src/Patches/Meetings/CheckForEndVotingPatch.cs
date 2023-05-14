using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Lotus.API.Odyssey;
using Lotus.API.Vanilla.Meetings;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Utilities;
using Lotus.Extensions;
using VentLib.Logging;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Harmony.Attributes;
using VentLib.Utilities.Optionals;
using static MeetingHud;

namespace Lotus.Patches.Meetings;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
public class CheckForEndVotingPatch
{
    public static bool Prefix(MeetingHud __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return true;
        MeetingDelegate meetingDelegate = MeetingDelegate.Instance;
        if (!meetingDelegate.IsForceEnd() && __instance.playerStates.Any(ps => !ps.AmDead && !ps.DidVote)) return false;
        VentLogger.Debug("Beginning End Voting", "CheckEndVotingPatch");

        // Calculate the exiled player once so that we can send the voting complete signal
        VentLogger.Trace($"End Vote Count: {meetingDelegate.CurrentVoteCount().Select(kv => $"{Utils.GetPlayerById(kv.Key).GetNameWithRole()}: {kv.Value}").Join()}");
        (byte exiledPlayer, bool isTie) = CalculateExiledPlayer(meetingDelegate);
        VentLogger.Trace($"Player With Most Votes: {Utils.PlayerById(exiledPlayer)}");
        
        // Set the meeting delegate exiled player since this is what we use to cascade information
        GameData.PlayerInfo? playerInfo = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(info => !isTie && info.PlayerId == exiledPlayer);
        meetingDelegate.ExiledPlayer = playerInfo;
        meetingDelegate.IsTie = isTie;
        
        ActionHandle handle = ActionHandle.NoInit();
        Game.TriggerForAll(RoleActionType.VotingComplete, ref handle, meetingDelegate);

        // WE DO NOT RECALCULATE THE EXILED PLAYER!
        // This means its up to roles that modify the meeting delegate to properly update the exiled player

        
        // Generate voter states to reflect voting
        List<VoterState> votingStates = GenerateVoterStates(meetingDelegate);
        
        __instance.RpcVotingComplete(votingStates.ToArray(), meetingDelegate.ExiledPlayer, meetingDelegate.IsTie);
        return false;
    }
    
    public static (byte exiledPlayer, bool isTie) CalculateExiledPlayer(MeetingDelegate meetingDelegate)
    {
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

        return (exiledPlayer, isTie);
    }

    private static List<VoterState> GenerateVoterStates(MeetingDelegate meetingDelegate)
    {
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
        return votingStates;
    }
    
    [QuickPrefix(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
    public static void VotingCompletePatch(MeetingHud __instance, [HarmonyArgument(1)] GameData.PlayerInfo? playerInfo)
    {
        if (AmongUsClient.Instance.AmHost) MeetingDelegate.Instance.ExiledPlayer = playerInfo;
    }
}