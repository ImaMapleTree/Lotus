using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TOHTOR.API.Reactive;
using TOHTOR.API.Reactive.HookEvents;
using TOHTOR.Extensions;
using TOHTOR.Factions.Impostors;
using TOHTOR.Gamemodes;
using TOHTOR.GUI.Name.Interfaces;
using TOHTOR.Managers;
using TOHTOR.Managers.History;
using TOHTOR.Player;
using TOHTOR.Roles;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.Subroles;
using TOHTOR.RPC;
using TOHTOR.Victory;
using VentLib.Networking.RPC.Attributes;
using VentLib.Utilities.Extensions;

namespace TOHTOR.API.Odyssey;

public static class Game
{
    private static ulong _gameID;

    public static DateTime StartTime;
    public static Dictionary<byte, PlayerPlus> Players = new();
    public static GameHistory GameHistory = null!;
    public static GameStates GameStates = null!;
    public static RandomSpawn RandomSpawn = null!;
    public static int RecursiveCallCheck;

    public static VanillaRoleTracker VanillaRoleTracker = null!;

    private static readonly Dictionary<byte, ulong> GameIDs = new();

    [ModRPC((uint) ModCalls.SetCustomRole, RpcActors.Host, RpcActors.NonHosts, MethodInvocation.ExecuteBefore)]
    public static void AssignRole(PlayerControl player, CustomRole role, bool sendToClient = false)
    {
        CustomRole assigned = CustomRoleManager.PlayersCustomRolesRedux[player.PlayerId] = role.Instantiate(player);
        if (State is GameState.InLobby or GameState.InIntro) player.GetTeamInfo().MyRole = role.RealRole;
        if (sendToClient) assigned.Assign();
    }

    [ModRPC((uint) ModCalls.SetSubrole, RpcActors.Host, RpcActors.NonHosts, MethodInvocation.ExecuteBefore)]
    public static void AssignSubrole(PlayerControl player, Subrole role, bool sendToClient = false)
    {
        Dictionary<byte, List<CustomRole>> playerSubroles = CustomRoleManager.PlayerSubroles;
        byte playerId = player.PlayerId;

        if (!playerSubroles.ContainsKey(playerId)) playerSubroles[playerId] = new List<CustomRole>();
        playerSubroles[playerId].Add((Subrole)role.Instantiate(player));
        if (sendToClient) role.Assign();
    }

    public static INameModel NameModel(this PlayerControl playerControl) => Players.GetValueOrDefault(playerControl.PlayerId, new PlayerPlus(playerControl)).NameModel;


    public static void RenderAllForAll(GameState? state = null, bool force = false) => Players.Values
        .Select(p => p.NameModel)
        .ForEach(n => Players.Values.ForEach(pp => n.RenderFor(pp.MyPlayer, state, true, force)));

    public static IEnumerable<PlayerControl> GetAllPlayers() => PlayerControl.AllPlayerControls.ToArray();
    public static IEnumerable<PlayerControl> GetAlivePlayers() => GetAllPlayers().Where(p => !p.Data.IsDead && !p.Data.Disconnected);
    public static IEnumerable<PlayerControl> GetDeadPlayers(bool disconnected = false) => GetAllPlayers().Where(p => p.Data.IsDead || (disconnected && p.Data.Disconnected));
    public static List<PlayerControl> GetAliveImpostors()
    {
        return GetAlivePlayers().Where(p => p.GetCustomRole().Faction is ImpostorFaction).ToList();
    }

    public static IEnumerable<PlayerControl> FindAlivePlayersWithRole(params CustomRole[] roles) =>
        GetAllPlayers()
            .Where(p => roles.Any(r => r.Is(p.GetCustomRole()) || p.GetSubroles().Any(s => s.Is(r))));

    public static void SyncAll() => GetAllPlayers().Do(p => p.GetCustomRole().SyncOptions());

    public static void TriggerForAll(RoleActionType action, ref ActionHandle handle, params object[] parameters) => GetAllPlayers().Trigger(action, ref handle, parameters);

    public static void Trigger(this IEnumerable<PlayerControl> players, RoleActionType action, ref ActionHandle handle, params object[] parameters)
    {
        if (action == RoleActionType.FixedUpdate)
            foreach (PlayerControl player in players) player.Trigger(action, ref handle, parameters);
        // Using a new Trigger algorithm to deal with ordering of triggers
        else
        {
            List<PlayerControl> allPlayers = players.ToList();
            handle.ActionType = action;
            parameters = parameters.AddToArray(handle);
            List<(RoleAction, AbstractBaseRole)> actionList = allPlayers.SelectMany(p => p.GetCustomRole().GetActions(action)).ToList();
            actionList.AddRange(allPlayers.SelectMany(p => p.GetSubroles().SelectMany(r => r.GetActions(action))));
            actionList.Sort((a1, a2) => a1.Item1.Priority.CompareTo(a2.Item1.Priority));
            foreach ((RoleAction roleAction, AbstractBaseRole role) in actionList)
            {
                if (role.MyPlayer != null && !role.MyPlayer.IsAlive() && !roleAction.TriggerWhenDead) return;
                roleAction.Execute(role, parameters);
            }

        }
    }

    public static string GetName(PlayerControl player)
    {
        return player == null ? "Unknown" : player.name;
    }

    public static ulong GetGameID(this PlayerControl player) => GameIDs.GetOrCompute(player.PlayerId, () => _gameID++);

    public static IGamemode CurrentGamemode => TOHPlugin.GamemodeManager.CurrentGamemode;

    //public static void ResetNames() => players.Values.Select(p => p.DynamicName).Do(name => name.ClearComponents());
    public static GameState State = GameState.InLobby;
    private static WinDelegate _winDelegate = new();

    public static WinDelegate GetWinDelegate() => _winDelegate;

    public static void Setup()
    {
        VanillaRoleTracker = new VanillaRoleTracker();
        _winDelegate = new WinDelegate();
        RandomSpawn = new RandomSpawn();
        StartTime = DateTime.Now;
        GameHistory = new();
        GameStates = new();
        Players.Clear();
        GetAllPlayers().Do(p => Players.Add(p.PlayerId, new PlayerPlus(p)));

        Hooks.GameStateHooks.GameStartHook.Propagate(new GameStateHookEvent());
        CurrentGamemode.SetupWinConditions(_winDelegate);
    }

    public static void Cleanup()
    {
        Players.Clear();
        CustomRoleManager.PlayersCustomRolesRedux.Clear();
        CustomRoleManager.PlayerSubroles.Clear();
        Hooks.GameStateHooks.GameEndHook.Propagate(new GameStateHookEvent());
        GameIDs.Clear();
    }
}

public enum GameState
{
    None,
    InIntro,
    InMeeting,
    InLobby,
    Roaming // When in Rome do as the Romans do
}