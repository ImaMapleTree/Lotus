using System;
using System.Collections.Generic;
using Lotus.Managers;
using Lotus.Managers.History;
using Lotus.Player;
using Lotus.Roles;
using Lotus.Roles.Overrides;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.API.Odyssey;

public class MatchData
{
    internal ulong MatchID;
    
    public GameHistory GameHistory = new();
    public DateTime StartTime = DateTime.Now;

    public Dictionary<ulong, FrozenPlayer> FrozenPlayers = new();
    public VanillaRoleTracker VanillaRoleTracker = new();
    
    public List<byte> UnreportableBodies = new();
    public int MeetingsCalled;
    
    
    public RoleData Roles = new();


    public class RoleData
    {
        public Dictionary<byte, CustomRole> MainRoles = new();
        public Dictionary<byte, List<CustomRole>> SubRoles = new();
        private readonly Dictionary<byte, RemoteList<GameOptionOverride>> rolePersistentOverrides = new();

        public Remote<GameOptionOverride> AddOverride(byte playerId, GameOptionOverride @override)
        {
            return rolePersistentOverrides.GetOrCompute(playerId, () => new RemoteList<GameOptionOverride>()).Add(@override);
        }

        public IEnumerable<GameOptionOverride> GetOverrides(byte playerId)
        {
            RemoteList<GameOptionOverride> remoteList = rolePersistentOverrides.GetOrCompute(playerId, () => new RemoteList<GameOptionOverride>());
            return remoteList;
        }

        public void AddMainRole(byte playerId, CustomRole role) => MainRoles[playerId] = role;
        public void AddSubrole(byte playerId, CustomRole subrole) => SubRoles.GetOrCompute(playerId, () => new List<CustomRole>()).Add(subrole);
        
        public CustomRole GetMainRole(byte playerId) => MainRoles.GetValueOrDefault(playerId, CustomRoleManager.Default);
        public List<CustomRole> GetSubroles(byte playerId) => SubRoles.GetOrCompute(playerId, () => new List<CustomRole>());
        
    }

    
}