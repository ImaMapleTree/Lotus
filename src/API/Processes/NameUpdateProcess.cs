using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using TOHTOR.API.Reactive;
using TOHTOR.Extensions;
using TOHTOR.GUI.Name.Interfaces;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Debug.Profiling;
using VentLib.Utilities.Extensions;

namespace TOHTOR.API.Processes;

[LoadStatic]
public class NameUpdateProcess
{
    private const string NameUpdateProcessHookKey = nameof(NameUpdateProcessHookKey);

    internal static bool Paused;
    private static Queue<PlayerControl> _players = new();

    static NameUpdateProcess()
    {
        Hooks.GameStateHooks.GameStartHook.Bind(NameUpdateProcessHookKey, _ => Game.GetAllPlayers().ForEach(p => _players.Enqueue(p))
        );
        Hooks.GameStateHooks.RoundStartHook.Bind(NameUpdateProcessHookKey, _ =>
        {
            if (!AmongUsClient.Instance.AmHost) return;
            Paused = false;
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

        Profiler.Sample sample = Profilers.Global.Sampler.StartQ();
        allPlayers.ForEach(p => nameModel.RenderFor(p));
        sample.Stop();

        Async.Schedule(NameUpdateLoop, 0.1f / allPlayers.Length);
    }

}