using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Utilities;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using static GameData;

namespace Lotus.Managers;

public static class AntiBlackoutLogic
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(AntiBlackoutLogic));

    public static HashSet<byte> PatchedData(byte exiledPlayer)
    {
        log.Debug("Patching GameData", "AntiBlackout");
        IEnumerable<PlayerControl> players = PlayerControl.AllPlayerControls.ToArray().Sorted(p => p.IsHost());
        VanillaRoleTracker roleTracker = Game.MatchData.VanillaRoleTracker;

        HashSet<byte> unpatchable = new();

        PlayerInfo[] allPlayers = Instance.AllPlayers.ToArray();

        foreach (PlayerControl player in players)
        {
            if (player.IsHost() || player.IsModded()) continue;
            log.Trace($"Patching For: {player.name} ({player.PrimaryRole().Name})", "AntiBlackout");
            ReviveEveryone(exiledPlayer);

            bool wasImpostor = roleTracker.GetAllImpostorIds(player.PlayerId).Contains(0);
            HashSet<byte> impostorIds = roleTracker.GetAllImpostorIds(player.PlayerId).Where(id => exiledPlayer != id && id != 0).ToHashSet();
            PlayerInfo[] impostorInfo = allPlayers.Where(info => impostorIds.Contains(info.PlayerId)).ToArray();
            log.Trace($"Impostors: {impostorInfo.Select(i => i.Object).Where(o => o != null).Select(o => o.name).Fuse()}");

            HashSet<byte> crewIds = roleTracker.GetAllCrewmateIds(player.PlayerId).Where(id => exiledPlayer != id).ToHashSet();
            PlayerInfo[] crewInfo = allPlayers.Where(info => crewIds.Contains(info.PlayerId)).ToArray();
            log.Trace($"Crew: {crewInfo.Select(i => i.Object).Where(o => o != null).Select(o => o.name).Fuse()}");

            int aliveImpostorCount = impostorInfo.Length;
            int aliveCrewCount = crewInfo.Length;

            if (player.PlayerId == exiledPlayer) { }
            else if (player.IsAlive() && player.GetVanillaRole().IsImpostor()) aliveImpostorCount++;
            else if (player.IsAlive()) aliveCrewCount++;
            if (wasImpostor && PlayerControl.LocalPlayer.GetVanillaRole().IsImpostor() && PlayerControl.LocalPlayer.PlayerId != exiledPlayer) aliveImpostorCount++;

            log.Trace($"Alive Crew: {aliveCrewCount} | Alive Impostors: {aliveImpostorCount}");

            bool IsFailure()
            {
                bool failure = false;
                if (aliveCrewCount == 0) failure = unpatchable.Add(player.PlayerId);
                if (aliveImpostorCount == 0) failure |= unpatchable.Add(player.PlayerId);
                return failure;
            }


            // Go until failure, or aliveCrew > aliveImpostor
            int index = 0;
            while (!IsFailure() && index < impostorInfo.Length)
            {
                if (aliveCrewCount > aliveImpostorCount) break;
                PlayerInfo info = impostorInfo[index++];
                if (info.Object != null) log.Trace($"Set {info.Object.name} => Disconnect = true | Impostors: {aliveImpostorCount - 1} | Crew: {aliveCrewCount}");
                info.Disconnected = true;
                aliveImpostorCount--;
            }

            // No matter what, if crew is less than impostor alive, we're unpatchable
            if (aliveCrewCount <= aliveImpostorCount) unpatchable.Add(player.PlayerId);


            GeneralRPC.SendGameData(player.GetClientId());
        }

        return unpatchable;
    }

    private static void ReviveEveryone(byte exiledPlayer) {
        foreach (var info in Instance.AllPlayers)
        {
            info.IsDead = false;
            info.Disconnected = false;
            if (info.PlayerId != exiledPlayer && info.Object != null) info.PlayerName = info.Object.name;
            else if (info.PlayerId != exiledPlayer) info.PlayerName = info.PlayerName.RemoveHtmlTags();
        }
    }
}