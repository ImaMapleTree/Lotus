using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Factions.Impostors;
using Lotus.Gamemodes;
using Lotus.GUI.Name.Impl;
using Lotus.GUI.Name.Interfaces;
using Lotus.Roles;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Victory;
using Lotus.Extensions;
using Lotus.Managers;
using VentLib.Logging;
using VentLib.Utilities.Extensions;

namespace Lotus.API.Odyssey;

public static class Game
{
    static Game()
    {
        Hooks.NetworkHooks.GameJoinHook.Bind("GameHook", ev =>
        {
            if (!ev.IsNewLobby) return;
            VentLogger.Trace("Joined new lobby. Cleaning up old game states.", "GameCleanupCheck");
            Cleanup(true);
        });
    }
    
    private static readonly Dictionary<byte, ulong> GameIDs = new();
    private static ulong _gameID;
    
    public static MatchData? LastMatch;
    public static MatchData MatchData = new();
    public static Dictionary<byte, INameModel> NameModels = new();
    public static RandomSpawn RandomSpawn = null!;
    public static int RecursiveCallCheck;

    public static GameState[] IgnStates => new[] { GameState.Roaming, GameState.InMeeting };

    public static INameModel NameModel(this PlayerControl playerControl) => NameModels.GetOrCompute(playerControl.PlayerId, () => new SimpleNameModel(playerControl));

    public static void RenderAllForAll(GameState? state = null, bool force = false) => NameModels.Values
        .ForEach(n => GetAllPlayers().ForEach(pp => n.RenderFor(pp, state, true, force)));

    public static IEnumerable<PlayerControl> GetAllPlayers() => PlayerControl.AllPlayerControls.ToArray();
    public static IEnumerable<PlayerControl> GetAlivePlayers() => GetAllPlayers().Where(p => p.IsAlive());

    public static IEnumerable<CustomRole> GetAliveRoles(bool includePhantomRoles = false)
    {
        IEnumerable<CustomRole> roles = GetAlivePlayers().Select(p => p.GetCustomRole());
        return includePhantomRoles ? roles : roles.Where(r => r is not IPhantomRole pr || pr.IsCountedAsPlayer());
    }


    public static IEnumerable<PlayerControl> GetDeadPlayers(bool disconnected = false) => GetAllPlayers().Where(p => p.Data.IsDead || (disconnected && p.Data.Disconnected));
    public static List<PlayerControl> GetAliveImpostors()
    {
        return GetAlivePlayers().Where(p => p.GetCustomRole().Faction is ImpostorFaction).ToList();
    }

    public static IEnumerable<PlayerControl> FindAlivePlayersWithRole(params CustomRole[] roles) =>
        GetAllPlayers()
            .Where(p => roles.Any(r => r.GetType() == p.GetCustomRole().GetType()) || p.GetSubroles().Any(s => s.GetType() == roles.GetType()));

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
                if (role.MyPlayer == null || !role.MyPlayer.IsAlive() && !roleAction.TriggerWhenDead) return;
                roleAction.Execute(role, parameters);
            }

        }
    }

    public static string GetName(PlayerControl player)
    {
        return player == null ? "Unknown" : player.name;
    }

    public static ulong GetGameID(this PlayerControl player) => GameIDs.GetOrCompute(player.PlayerId, () => _gameID++);
    public static ulong GetGameID(byte playerId) => GameIDs.GetOrCompute(playerId, () => _gameID++);

    public static ulong NextMatchID() => MatchData.MatchID++;

    public static IGamemode CurrentGamemode => ProjectLotus.GamemodeManager.CurrentGamemode;

    //public static void ResetNames() => players.Values.Select(p => p.DynamicName).Do(name => name.ClearComponents());
    public static GameState State = GameState.InLobby;
    private static WinDelegate _winDelegate = new();

    public static WinDelegate GetWinDelegate() => _winDelegate;

    public static void Setup()
    {
        LastMatch = MatchData;
        MatchData = new MatchData();
        _winDelegate = new WinDelegate();
        RandomSpawn = new RandomSpawn();
        NameModels.Clear();
        GetAllPlayers().Do(p => NameModels.Add(p.PlayerId, new SimpleNameModel(p)));

        Hooks.GameStateHooks.GameStartHook.Propagate(new GameStateHookEvent(MatchData));
        CurrentGamemode.SetupWinConditions(_winDelegate);
    }

    public static void Cleanup(bool newLobby = false)
    {
        NameModels.Clear();
        if (newLobby) MatchData = new MatchData();
        Hooks.GameStateHooks.GameEndHook.Propagate(new GameStateHookEvent(MatchData));
        State = GameState.InLobby;
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