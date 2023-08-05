using Lotus.API.Odyssey;
using Lotus.API.Vanilla;
using Lotus.API.Vanilla.Meetings;
using Lotus.Chat.Commands;
using Lotus.Roles.Interactions;
using Lotus.Extensions;
using Lotus.GUI.Menus.OptionsMenu.Patches;
using Lotus.Logging;
using Lotus.Patches.Client;
using Lotus.Roles;
using LotusTrigger.Options;
using UnityEngine;
using VentLib.Localization;
using VentLib.Options;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Debug.Profiling;
using static Lotus.Managers.Hotkeys.HotkeyManager;

namespace Lotus.Managers.Hotkeys;

[LoadStatic]
public class ModKeybindings
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(ModKeybindings));

    private static bool hudActive = true;

    static ModKeybindings()
    {
        // Dump Log
        Bind(KeyCode.F, KeyCode.LeftControl).Do(DumpLog);
        Bind(KeyCode.D, KeyCode.LeftControl).Do(() => LogManager.WriteSessionLog(""));

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
            .If(p => p.HostOnly().Predicate(() => MatchState.IsCountDown && !HudManager.Instance.Chat.IsOpenOrOpening))
            .Do(() => GameStartManager.Instance.countDownTimer = 0);

        // Restart countdown timer
        Bind(KeyCode.C)
            .If(p => p.HostOnly().Predicate(() => MatchState.IsCountDown && !HudManager.Instance.Chat.IsOpenOrOpening))
            .Do(() =>
            {
                GeneralOptions.AdminOptions.AutoStartMaxTime = -1;
                GameStartManager.Instance.ResetStartState();
            });

        Bind(KeyCode.C)
            .If(p => p.HostOnly().Predicate(() => EndGameManagerPatch.IsRestarting))
            .Do(EndGameManagerPatch.CancelPlayAgain);

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

        Bind(KeyCode.F7)
            .If(p => p.State(GameState.InLobby, GameState.Roaming).Predicate(() => MeetingHud.Instance == null))
            .Do(() => HudManager.Instance.gameObject.SetActive(hudActive = !hudActive));

        Bind(KeyCode.Escape)
            .If(p => p.Predicate(() => GameOptionMenuOpenPatch.MenuBehaviour != null && GameOptionMenuOpenPatch.MenuBehaviour.IsOpen))
            .Do(() => GameOptionMenuOpenPatch.MenuBehaviour.Close());
    }

    private static void DumpLog()
    {
        LogManager.OpenLogUI();
        /*BasicCommands.Dump(PlayerControl.LocalPlayer);*/
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
        PlayerControl.LocalPlayer.InteractWith(PlayerControl.LocalPlayer, LotusInteraction.FatalInteraction.Create(PlayerControl.LocalPlayer));
    }

    private static void ResetGameOptions()
    {
        log.High("Resetting Game Options", "ResetOptions");
        OptionManager.GetAllManagers().ForEach(m =>
        {
            m.GetOptions().ForEach(o =>
            {
                o.SetValue(o.DefaultIndex);
                OptionHelpers.GetChildren(o).ForEach(o2 => o2.SetValue(o2.DefaultIndex));
            });
            m.DelaySave(0);
        });
        StaticLogger.SendInGame("All options have been reset!");
    }

    private static void InstantReduceTimer()
    {
        PlayerControl.LocalPlayer.SetKillCooldown(0f);
    }

    private static void ReloadTranslations()
    {
        log.Trace("Reload Custom Translation File", "KeyCommand");
        Localizer.Reload();
        StaticLogger.SendInGame("Reloaded Custom Translation File");
    }
}