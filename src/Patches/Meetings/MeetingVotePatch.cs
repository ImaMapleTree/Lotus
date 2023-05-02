using HarmonyLib;
using Hazel;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.API.Vanilla.Meetings;
using TOHTOR.Extensions;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Utilities;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Optionals;

namespace TOHTOR.Patches.Meetings;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote))]
public class MeetingVotePatch
{
    public static void Prefix(MeetingHud __instance, byte srcPlayerId, byte suspectPlayerId)
    {
        PlayerControl voter = Utils.GetPlayerById(srcPlayerId)!;
        Optional<PlayerControl> voted = Utils.PlayerById(suspectPlayerId);
        VentLogger.Trace($"{voter.GetNameWithRole()} voted for {voted.Map(v => v.name)}");

        ActionHandle handle = ActionHandle.NoInit();
        voter.Trigger(RoleActionType.MyVote, ref handle,MeetingDelegate.Instance, voted);
        Game.TriggerForAll(RoleActionType.AnyVote, ref handle, MeetingDelegate.Instance, voter, voted);

        if (!handle.IsCanceled)
        {
            MeetingDelegate.Instance.AddVote(voter, voted);
            return;
        }

        VentLogger.Debug($"Canceled Vote from {voter.GetNameWithRole()}");
        Async.Schedule(() => ClearVote(__instance, voter), NetUtils.DeriveDelay(0.4f));
        Async.Schedule(() => ClearVote(__instance, voter), NetUtils.DeriveDelay(0.6f));
    }

    private static void ClearVote(MeetingHud hud, PlayerControl target)
    {
        VentLogger.Trace($"Clearing vote for: {target.GetNameWithRole()}");
        /*PlayerVoteArea voteArea = hud.playerStates.ToArray().FirstOrDefault(state => state.TargetPlayerId == target.PlayerId)!;
        voteArea.UnsetVote();*/
        MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
        writer.StartMessage(6);
        writer.Write(AmongUsClient.Instance.GameId);
        writer.WritePacked(target.GetClientId());
        {
            writer.StartMessage(2);
            writer.WritePacked(hud.NetId);
            writer.WritePacked((uint)RpcCalls.ClearVote);
        }

        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
}