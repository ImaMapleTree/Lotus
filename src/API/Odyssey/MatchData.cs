using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Player;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Extensions;
using Lotus.Managers;
using Lotus.Managers.History;
using Lotus.Roles;
using Lotus.Roles.Overrides;
using Lotus.RPC;
using Lotus.Statuses;
using MonoMod.RuntimeDetour;
using VentLib.Networking.RPC.Attributes;
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

    public HashSet<byte> UnreportableBodies = new();
    public int MeetingsCalled;
    public int EmergencyButtonsUsed;

    private static readonly Func<RemoteList<GameOptionOverride>> OptionOverrideListSupplier =
        () => AUSettings.ConfirmImpostor() ? new RemoteList<GameOptionOverride> { new(Override.ConfirmEjects, false) } : new RemoteList<GameOptionOverride>();


    public RoleData Roles = new();

    public FrozenPlayer? FrozenPlayer(PlayerControl? player)
    {
        return player == null || !FrozenPlayers.ContainsKey(player.GetGameID()) ? null : FrozenPlayers[player.GetGameID()];
    }

    public void Cleanup()
    {
        UnreportableBodies.Clear();
        Roles = new RoleData();
    }

    public class RoleData
    {
        public Dictionary<byte, CustomRole> MainRoles = new();
        public Dictionary<byte, List<CustomRole>> SubRoles = new();


        private readonly Dictionary<byte, RemoteList<GameOptionOverride>> rolePersistentOverrides = new();

        public Remote<GameOptionOverride> AddOverride(byte playerId, GameOptionOverride @override)
        {
            return rolePersistentOverrides.GetOrCompute(playerId, OptionOverrideListSupplier).Add(@override);
        }

        public IEnumerable<GameOptionOverride> GetOverrides(byte playerId)
        {
            return rolePersistentOverrides.GetOrCompute(playerId, OptionOverrideListSupplier);
        }

        public CustomRole AddMainRole(byte playerId, CustomRole role) => MainRoles[playerId] = role;
        public void AddSubrole(byte playerId, CustomRole subrole) => SubRoles.GetOrCompute(playerId, () => new List<CustomRole>()).Add(subrole);

        public CustomRole GetMainRole(byte playerId) => MainRoles.GetValueOrDefault(playerId, CustomRoleManager.Default);
        public List<CustomRole> GetSubroles(byte playerId) => SubRoles.GetOrCompute(playerId, () => new List<CustomRole>());

    }


    public static List<CustomRole> GetEnabledRoles() => CustomRoleManager.AllRoles.Where(r => r.IsEnabled()).ToList();

    [ModRPC((uint) ModCalls.SetCustomRole, RpcActors.Host, RpcActors.NonHosts, MethodInvocation.ExecuteBefore)]
    public static void AssignRole(PlayerControl player, CustomRole role, bool sendToClient = false)
    {
        CustomRole assigned = Game.MatchData.Roles.AddMainRole(player.PlayerId, role.Instantiate(player));
        Game.MatchData.FrozenPlayers.GetOptional(player.GetGameID()).IfPresent(fp => fp.Role = assigned);
        if (Game.State is GameState.InLobby or GameState.InIntro) player.GetTeamInfo().MyRole = role.RealRole;
        if (sendToClient) assigned.Assign();
    }

    [ModRPC((uint) ModCalls.SetSubrole, RpcActors.Host, RpcActors.NonHosts, MethodInvocation.ExecuteBefore)]
    public static void AssignSubrole(PlayerControl player, CustomRole role, bool sendToClient = false)
    {
        CustomRole instantiated = role.Instantiate(player, true);
        Game.MatchData.Roles.AddSubrole(player.PlayerId, instantiated);
        if (sendToClient) role.Assign();
    }

    public static RemoteList<IStatus>? GetStatuses(PlayerControl player)
    {
        return Game.MatchData.FrozenPlayers.GetOptional(player.GetGameID()).Map(fp => fp.Statuses).OrElse(null!);
    }

    public static Remote<IStatus>? AddStatus(PlayerControl player, IStatus status, PlayerControl? infector = null)
    {
        return Game.MatchData.FrozenPlayers.GetOptional(player.GetGameID()).Map(fp => fp.Statuses).Transform(statuses =>
        {
            Remote<IStatus> remote = statuses.Add(status);
            Hooks.ModHooks.StatusReceivedHook.Propagate(new PlayerStatusReceivedHook(player, status, infector));
            return remote;
        }, () =>  null!);
    }
}