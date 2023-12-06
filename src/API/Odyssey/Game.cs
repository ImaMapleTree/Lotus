using System.Collections.Generic;
using HarmonyLib;
using Lotus.API.Player;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.GameModes;
using Lotus.GUI.Name.Impl;
using Lotus.GUI.Name.Interfaces;
using Lotus.Victory;
using Lotus.Extensions;
using VentLib.Utilities.Extensions;

namespace Lotus.API.Odyssey;

public static class Game
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(Game));

    private static readonly Dictionary<byte, ulong> GameIDs = new();
    private static ulong _gameID;

    public static MatchData MatchData => CurrentGameMode.MatchData;
    public static Dictionary<byte, INameModel> NameModels = new();
    public static RandomSpawn RandomSpawn = null!;
    public static int RecursiveCallCheck;
    public static IGameMode CurrentGameMode => ProjectLotus.GameModeManager.CurrentGameMode;
    public static GameState State = GameState.InLobby;
    private static WinDelegate _winDelegate = new();

    static Game()
    {
        Hooks.NetworkHooks.GameJoinHook.Bind("GameHook", ev =>
        {
            if (!ev.IsNewLobby) return;
            log.Trace("Joined new lobby. Cleaning up old game states.", "GameCleanupCheck");
            Cleanup(true);
        });
    }

    public static ulong NextMatchID() => MatchData.MatchID++;

    public static GameState[] IgnStates => new[] { GameState.Roaming, GameState.InMeeting };

    public static INameModel NameModel(this PlayerControl playerControl) => NameModels.GetOrCompute(playerControl.PlayerId, () => new SimpleNameModel(playerControl));

    public static void RenderAllForAll(GameState? state = null, bool force = false) => NameModels.Values.ForEach(n => Players.GetPlayers().ForEach(pp => n.RenderFor(pp, state, true, force)));

    public static void SyncAll() => Players.GetPlayers().Do(p => p.SyncAll());

    public static string GetName(PlayerControl player)
    {
        return player == null ? "Unknown" : player.name;
    }

    public static ulong GetGameID(this PlayerControl player) => GameIDs.GetOrCompute(player.PlayerId, () => _gameID++);
    public static ulong GetGameID(byte playerId) => GameIDs.GetOrCompute(playerId, () => _gameID++);

    public static WinDelegate GetWinDelegate() => _winDelegate;

    public static void Setup()
    {
        CurrentGameMode.MatchData = new MatchData();
        _winDelegate = new WinDelegate();
        RandomSpawn = new RandomSpawn();
        NameModels.Clear();
        Players.GetPlayers().Do(p => NameModels.Add(p.PlayerId, new SimpleNameModel(p)));

        Hooks.GameStateHooks.GameStartHook.Propagate(new GameStateHookEvent(MatchData, CurrentGameMode));
        ProjectLotus.GameModeManager.StartGame(_winDelegate);
    }

    public static void Cleanup(bool newLobby = false)
    {
        NameModels.Clear();
        if (newLobby) MatchData.Cleanup();
        Hooks.GameStateHooks.GameEndHook.Propagate(new GameStateHookEvent(MatchData, CurrentGameMode));
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