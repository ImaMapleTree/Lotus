using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Lotus.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.API.Odyssey;

public class VanillaRoleTracker
{
    private Dictionary<byte, TeamInfo> roleDictionary = new();

    public TeamInfo GetInfo(byte player)
    {
        return roleDictionary.GetOrCompute(player, () => new TeamInfo(player));
    }

    public IEnumerable<PlayerControl> GetAllImpostors(byte playerId)
    {
        return GetInfo(playerId).Impostors.Select(i => Utils.PlayerById(i)).Filter();
    }

    public HashSet<byte> GetAllImpostorIds(byte playerId) => GetInfo(playerId).Impostors;

    public IEnumerable<PlayerControl> GetAllCrewmates(byte playerId)
    {
        return GetInfo(playerId).Crewmates.Select(i => Utils.PlayerById(i)).Filter();
    }

    public HashSet<byte> GetAllCrewmateIds(byte playerId) => GetInfo(playerId).Crewmates;

    public class TeamInfo
    {
        public HashSet<byte> Impostors = new();
        public HashSet<byte> Crewmates = new();
        public RoleTypes MyRole { get; set; } // TODO: change privacy again
        private byte myPlayer;

        public TeamInfo(byte myPlayer)
        {
            this.myPlayer = myPlayer;
            Game.MatchData.Roles.MainRoles.GetOptional(myPlayer).IfPresent(role => MyRole = role.RealRole);
        }

        public void AddVanillaCrewmate(byte playerId)
        {
            if (playerId == myPlayer) return;
            Impostors.Remove(playerId);
            Crewmates.Add(playerId);
        }

        public void AddVanillaImpostor(byte playerId)
        {
            if (playerId == myPlayer) return;
            Crewmates.Remove(playerId);
            Impostors.Add(playerId);
        }

        public void AddPlayer(byte playerId, bool isImpostor)
        {
            if (playerId == myPlayer) return;
            if (isImpostor) AddVanillaImpostor(playerId);
            else AddVanillaCrewmate(playerId);
        }

        public override string ToString()
        {
            return $"TeamInfo(player={myPlayer}, role={MyRole})";
        }
    }
}
