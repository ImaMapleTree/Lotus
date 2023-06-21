using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Player;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Gamemodes;
using Lotus.GUI.Name.Impl;
using Lotus.GUI.Name.Interfaces;
using Lotus.Roles;
using Lotus.Roles.Internals;
using Lotus.Victory;
using Lotus.Extensions;
using Lotus.Roles.Internals.Enums;
using VentLib.Logging;
using VentLib.Utilities.Extensions;

namespace Lotus.API.Odyssey;

public static class Game
{
    private static readonly Dictionary<byte, ulong> GameIDs = new();
    private static ulong _gameID;

    public static MatchData MatchData = new();
    public static Dictionary<byte, INameModel> NameModels = new();
    public static RandomSpawn RandomSpawn = null!;
    public static int RecursiveCallCheck;
    public static IGamemode CurrentGamemode => ProjectLotus.GamemodeManager.CurrentGamemode;
    public static GameState State = GameState.InLobby;
    private static WinDelegate _winDelegate = new();

    static Game()
    {
        Hooks.NetworkHooks.GameJoinHook.Bind("GameHook", ev =>
        {
            if (!ev.IsNewLobby) return;
            VentLogger.Trace("Joined new lobby. Cleaning up old game states.", "GameCleanupCheck");
            Cleanup(true);
        });
    }

    public static ulong NextMatchID() => MatchData.MatchID++;

    public static GameState[] IgnStates => new[] { GameState.Roaming, GameState.InMeeting };

    public static INameModel NameModel(this PlayerControl playerControl) => NameModels.GetOrCompute(playerControl.PlayerId, () => new SimpleNameModel(playerControl));

    public static void RenderAllForAll(GameState? state = null, bool force = false) => NameModels.Values.ForEach(n => Players.GetPlayers().ForEach(pp => n.RenderFor(pp, state, true, force)));

    public static void SyncAll() => Players.GetPlayers().Do(p => p.SyncAll());

    public static void TriggerForAll(RoleActionType action, ref ActionHandle handle, params object[] parameters)
    {
        foreach (PlayerControl player in PlayerControl.AllPlayerControls) player.Trigger(action, ref handle, parameters);
    }

    public static void TriggerForAll(this IEnumerable<PlayerControl> players, RoleActionType action, ref ActionHandle handle, params object[] parameters)
    {
        foreach (PlayerControl player in players) player.Trigger(action, ref handle, parameters);
    }

    public static void TriggerOrdered(this IEnumerable<PlayerControl> players, RoleActionType action, ref ActionHandle handle, params object[] parameters)
    {
        if (action is RoleActionType.FixedUpdate)
            foreach (PlayerControl player in players) player.Trigger(action, ref handle, parameters);
        // Using a new Trigger algorithm to deal with ordering of triggers
        else
        {
            List<PlayerControl> allPlayers = players.ToList();
            handle.ActionType = action;
            parameters = parameters.AddToArray(handle);
            List<(RoleAction, AbstractBaseRole)> actionList = allPlayers.SelectMany(p => p.GetCustomRole().GetActions(action)).ToList();
            actionList.AddRange(allPlayers.SelectMany(p => p.GetSubroles().SelectMany(r => r.GetActions(action))));
            /*VentLogger.Debug($"All Actions: {actionList.Select(a => a.Item1.ToString()).Fuse()}");*/
            foreach ((RoleAction roleAction, AbstractBaseRole role) in actionList.OrderBy(a1 => a1.Item1.Priority))
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

    public static WinDelegate GetWinDelegate() => _winDelegate;

    public static void Setup()
    {
        MatchData = new MatchData();
        _winDelegate = new WinDelegate();
        RandomSpawn = new RandomSpawn();
        NameModels.Clear();
        Players.GetPlayers().Do(p => NameModels.Add(p.PlayerId, new SimpleNameModel(p)));

        Hooks.GameStateHooks.GameStartHook.Propagate(new GameStateHookEvent(MatchData));
        CurrentGamemode.SetupWinConditions(_winDelegate);
    }

    public static void Cleanup(bool newLobby = false)
    {
        NameModels.Clear();
        if (newLobby) MatchData.Cleanup();
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