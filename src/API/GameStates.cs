using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Extensions;

namespace Lotus.API;

public static class GameStates
{
    public static GameState[] IgnStates = { GameState.InMeeting, GameState.Roaming };

    public static bool InGame = false;
    public static bool IsLobby => AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Joined;
    public static bool IsInGame => InGame;
    public static bool IsOnlineGame => AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame;
    public static bool IsFreePlay => AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;
    public static bool IsInTask => InGame && !MeetingHud.Instance;
    public static bool IsMeeting => InGame && MeetingHud.Instance;
    public static bool IsCountDown => GameStartManager.InstanceExists && GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown;

    public static int CountAliveImpostors() => Game.GetAliveImpostors().Count;
    public static int CountAliveRealImpostors() => Game.GetAlivePlayers().Count(p => p.GetCustomRole().RealRole.IsImpostor());
    public static int CountRealImpostors() => Game.GetAllPlayers().Count(p => p.GetCustomRole().RealRole.IsImpostor());
    public static int CountAliveRealCrew() => Game.GetAlivePlayers().Count(p => p.GetCustomRole().RealRole.IsCrewmate());
    public static int CountRealCrew() => Game.GetAllPlayers().Count(p => p.GetCustomRole().RealRole.IsCrewmate());
    
    
}