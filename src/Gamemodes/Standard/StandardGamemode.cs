using System;
using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.API.Reactive;
using TOHTOR.API.Reactive.HookEvents;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.Factions.Interfaces;
using TOHTOR.Factions.Neutrals;
using TOHTOR.Gamemodes.Standard.WinCons;
using TOHTOR.Options;
using TOHTOR.Options.Roles;
using TOHTOR.Roles.Internals;
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
        Game.GetWinDelegate().AddSubscriber(FixNeutralTeamingWinners);
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
        winDelegate.AddWinCondition(new SoloKillingWinCondition());
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

    public static void ShowInformationToGhost(PlayerHookEvent hookEvent)
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

    private static void FixNeutralTeamingWinners(WinDelegate winDelegate)
    {
        if (RoleOptions.NeutralOptions.NeutralTeamingMode is NeutralTeaming.Disabled) return;
        if (winDelegate.GetWinners().Count != 1) return;
        List<PlayerControl> winners = winDelegate.GetWinners();
        PlayerControl winner = winners[0];
        if (winner.GetCustomRole().Faction is not Solo) return;

        winners.AddRange(Game.GetAllPlayers()
            .Where(p => p.PlayerId != winner.PlayerId)
            .Where(p => winner.Relationship(p) is Relation.SharedWinners or Relation.FullAllies));
    }
}