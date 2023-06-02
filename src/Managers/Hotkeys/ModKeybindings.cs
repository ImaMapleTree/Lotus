using System.IO;
using Lotus.API.Odyssey;
using Lotus.API.Vanilla;
using Lotus.API.Vanilla.Meetings;
using Lotus.Chat.Commands;
using Lotus.Roles.Interactions;
using Lotus.Extensions;
using Lotus.Roles;
using UnityEngine;
using VentLib.Localization;
using VentLib.Logging;
using VentLib.Options;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Debug.Profiling;
using VentLib.Utilities.Extensions;
using static Lotus.Managers.Hotkeys.HotkeyManager;

namespace Lotus.Managers.Hotkeys;

[LoadStatic]
public class ModKeybindings
{
    private static bool hudActive = true;

    static ModKeybindings()
    {
        // Dump Log
        Bind(KeyCode.F1, KeyCode.LeftControl).Do(DumpLog);

        // Profile All
        Bind(KeyCode.F2).Do(ProfileAll);

        // Kill Player (Suicide)
        Bind(KeyCode.LeftShift, KeyCode.D, KeyCode.Return)
            .If(p => p.HostOnly().State(Game.IgnStates))
            .Do(Suicide);

        // Close Meeting
        Bind(KeyCode.LeftShift, KeyCode.M, KeyCode.Return)
            .If(p => p.HostOnly().State(GameState.InMeeting))
            .Do(() => MeetingHud.Instance.RpcClose());

        // Instant begin game
        Bind(KeyCode.LeftShift)
            .If(p => p.HostOnly().Predicate(() => MatchState.IsCountDown))
            .Do(() => GameStartManager.Instance.countDownTimer = 0);

        // Restart countdown timer
        Bind(KeyCode.C)
            .If(p => p.HostOnly().Predicate(() => MatchState.IsCountDown))
            .Do(() => GameStartManager.Instance.ResetStartState());

        // Reset Game Options
        Bind(KeyCode.LeftControl, KeyCode.Delete)
            .If(p => p.Predicate(() => Object.FindObjectOfType<GameOptionsMenu>()))
            .Do(ResetGameOptions);

        // Instant call meeting
        Bind(KeyCode.RightShift, KeyCode.M, KeyCode.Return)
            .If(p => p.HostOnly().State(GameState.Roaming))
            .Do(() => MeetingPrep.PrepMeeting(PlayerControl.LocalPlayer));

        // Sets kill cooldown to 0
        Bind(KeyCode.X)
            .If(p => p.HostOnly().State(GameState.Roaming))
            .Do(InstantReduceTimer);

        Bind(KeyCode.LeftControl, KeyCode.T)
            .If(p => p.State(GameState.InLobby))
            .Do(ReloadTranslations);

        Bind(KeyCode.F7).Do(() => HudManager.Instance.gameObject.SetActive(hudActive = !hudActive));
    }

    private static void DumpLog()
    {
        BasicCommands.Dump(PlayerControl.LocalPlayer);
    }

    private static void ProfileAll()
    {
        Profilers.All.ForEach(p =>
        {
            p.Display();
            p.Clear();
        });
    }

    private static void Suicide()
    {
        PlayerControl.LocalPlayer.InteractWith(PlayerControl.LocalPlayer, DirectInteraction.FatalInteraction.Create(PlayerControl.LocalPlayer));
    }

    private static void ResetGameOptions()
    {
        VentLogger.High("Resetting Game Options", "ResetOptions");
        OptionManager.GetAllManagers().ForEach(m =>
        {
            m.GetOptions().ForEach(o =>
            {
                o.SetValue(o.DefaultIndex);
                OptionHelpers.GetChildren(o).ForEach(o2 => o2.SetValue(o.DefaultIndex));
            });
            m.DelaySave(0);
        });
    }

    private static void InstantReduceTimer()
    {
        PlayerControl.LocalPlayer.SetKillCooldown(0f);
    }

    private static void ReloadTranslations()
    {
        VentLogger.Trace("Reload Custom Translation File", "KeyCommand");
        Localizer.Reload();
        VentLogger.SendInGame("Reloaded Custom Translation File");
    }
}