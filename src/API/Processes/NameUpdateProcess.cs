using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.GUI.Name.Interfaces;
using Lotus.Extensions;
using Lotus.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Debug.Profiling;
using VentLib.Utilities.Extensions;

namespace Lotus.API.Processes;

[LoadStatic]
public class NameUpdateProcess
{
    private const string NameUpdateProcessHookKey = nameof(NameUpdateProcessHookKey);

    internal static bool Paused;
    private static Queue<PlayerControl> _players = new();

    private static int _forceFixCount;
    private static readonly HashSet<byte> ForceFixedPlayers = new();

    static NameUpdateProcess()
    {
        Hooks.GameStateHooks.GameStartHook.Bind(NameUpdateProcessHookKey, _ => Game.GetAllPlayers().ForEach(p => _players.Enqueue(p)));
        Hooks.GameStateHooks.RoundStartHook.Bind(NameUpdateProcessHookKey, _ =>
        {
            if (!AmongUsClient.Instance.AmHost) return;
            Paused = false;
            Async.Schedule(() =>
            {
                ForceFixedPlayers.Clear();
                _forceFixCount = _players.Count;
            }, 1f);
            NameUpdateLoop();
        });
        Hooks.GameStateHooks.RoundEndHook.Bind(NameUpdateProcessHookKey, _ => Paused = true);
        Hooks.GameStateHooks.GameEndHook.Bind(NameUpdateProcessHookKey, _ => Paused = true);
    }

    public static void NameUpdateLoop()
    {
        if (_players.Count == 0) _players = new Queue<PlayerControl>(PlayerControl.AllPlayerControls.ToArray());

        if (Paused || Game.State is GameState.InMeeting)
        {
            Paused = false;
            return;
        }

        PlayerControl player = _players.Dequeue();

        if (player == null || player.Data.Disconnected)
        {
            Async.Schedule(NameUpdateLoop, 0.05f);
            return;
        }

        INameModel nameModel = player.NameModel();
        Il2CppArrayBase<PlayerControl> allPlayers = PlayerControl.AllPlayerControls.ToArray();
        if (allPlayers.Length == 0) return;

        Profiler.Sample sample = Profilers.Global.Sampler.Sampled();
        bool updated = false;
        allPlayers.ForEach(p =>
        {
            nameModel.RenderFor(p);
            updated |= (nameModel.Updated() && !player.IsAlive());
        });
        sample.Stop();

        Async.Schedule(NameUpdateLoop, 0.1f / allPlayers.Length);
        if (player.IsAlive()) return;
        if (_forceFixCount-- <= 0 || ForceFixedPlayers.Contains(player.PlayerId))
            if (!updated) return;
        DevLogger.Log($"Fixing for: {player.name}");
        ForceFixedPlayers.Add(player.PlayerId);
        player.SetChatName(player.name);
    }

}