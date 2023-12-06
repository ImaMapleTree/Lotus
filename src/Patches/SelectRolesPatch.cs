using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Extensions;
using Lotus.Managers;
using Lotus.Roles2.Manager;
using LotusTrigger.Options;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using GameMaster = Lotus.Roles2.Definitions.GameMaster;

namespace Lotus.Patches;

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
class SelectRolesPatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(SelectRolesPatch));
    private static bool encounteredError;

    public static void Prefix()
    {
        encounteredError = false;
        if (!AmongUsClient.Instance.AmHost) return;
        try
        {
            List<PlayerControl> unassignedPlayers = Players.GetPlayers().ToList();
            if (GeneralOptions.AdminOptions.HostGM)
            {
                MatchData.AssignRole(PlayerControl.LocalPlayer, IRoleManager.Current.GetRole<GameMaster>(), true);
                unassignedPlayers.RemoveAll(p => p.PlayerId == PlayerControl.LocalPlayer.PlayerId);
            }

            Game.CurrentGameMode.AssignRoles(unassignedPlayers);
        }
        catch (Exception exception)
        {
            encounteredError = true;
            FatalErrorHandler.ForceEnd(exception, "Assignment Phase");
        }
    }

    public static void Postfix()
    {
        if (!AmongUsClient.Instance.AmHost || encounteredError) return;

        Players.GetPlayers().Do(p => p.PrimaryRole().SyncOptions());

        TextTable textTable = new("ID", "Color", "Player", "Role", "SubRoles");
        Players.GetPlayers().Where(p => p != null).ForEach(p =>
        {
            textTable.AddEntry((object)p.PlayerId, ModConstants.ColorNames[p.cosmetics.ColorId], p.name, p.PrimaryRole().Name, p.SecondaryRoles().Fuse());
        });
        log.Debug($"Role Assignments\n{textTable}", "RoleManager::SelectRoles~Postfix");
        Game.RenderAllForAll(state: GameState.InIntro);
    }
}