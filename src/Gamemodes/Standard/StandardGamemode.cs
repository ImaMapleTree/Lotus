using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.API.Reactive;
using TOHTOR.API.Reactive.HookEvents;
using TOHTOR.Extensions;
using TOHTOR.Factions.Interfaces;
using TOHTOR.Options;
using TOHTOR.Roles.RoleGroups.Undead;
using TOHTOR.Victory;
using TOHTOR.Victory.Conditions;
using VentLib.Logging;
using VentLib.Options.Game.Tabs;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Gamemodes.Standard;

public class StandardGamemode: Gamemode
{
    private const string StandardGamemodeHookKey = nameof(StandardGamemodeHookKey);

    public override string GetName() => "Standard";

    public override void Setup()
    {
    }

    public override void AssignRoles(List<PlayerControl> players)
    {
        StandardRoleAssignmentLogic.AssignRoles(players);
    }

    public override IEnumerable<GameOptionTab> EnabledTabs() => DefaultTabs.All;

    public override void SetupWinConditions(WinDelegate winDelegate)
    {
        winDelegate.AddWinCondition(new VanillaCrewmateWin());
        winDelegate.AddWinCondition(new VanillaImpostorWin());
        winDelegate.AddWinCondition(new SabotageWin());
        winDelegate.AddWinCondition(new StandardWinConditions.LoversWin());
        winDelegate.AddWinCondition(new StandardWinConditions.SoloKillingWin());
        winDelegate.AddWinCondition(new StandardWinConditions.SoloRoleWin());
        winDelegate.AddWinCondition(new UndeadWinCondition());
    }

    public override void Activate()
    {
        Hooks.PlayerHooks.PlayerDeathHook.Bind(StandardGamemodeHookKey, ShowInformationToGhost);
    }

    public override void Deactivate()
    {
        Hooks.UnbindAll(StandardGamemodeHookKey);
    }

    private void ShowInformationToGhost(PlayerHookEvent hookEvent)
    {
        PlayerControl player = hookEvent.Player;
        if (player == null) return;

        VentLogger.Trace($"Showing all name components to ghost {player.name}", "GhostNameViewer");

        Game.GetAllPlayers().Where(p => p.PlayerId != player.PlayerId)
            .SelectMany(p => p.NameModel().ComponentHolders())
            .ForEach(holders =>
            {
                holders.AddListener(component => component.AddViewer(player));
                holders.Components().ForEach(components => components.AddViewer(player));
            }
        );
    }
}