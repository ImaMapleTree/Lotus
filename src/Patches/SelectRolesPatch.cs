using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Extensions;
using LotusTrigger.Options;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Patches;

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
class SelectRolesPatch
{
    public static void Prefix()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        List<PlayerControl> unassignedPlayers = Players.GetPlayers().ToList();
        if (GeneralOptions.AdminOptions.HostGM)
        {
            MatchData.AssignRole(PlayerControl.LocalPlayer, ProjectLotus.RoleManager.Internal.GameMaster, true);
            unassignedPlayers.RemoveAll(p => p.PlayerId == PlayerControl.LocalPlayer.PlayerId);
        }
        Game.CurrentGamemode.AssignRoles(unassignedPlayers);
    }

    public static void Postfix()
    {
        if (!AmongUsClient.Instance.AmHost) return;

        Players.GetPlayers().Do(p => p.GetCustomRole().SyncOptions());

        TextTable textTable = new("ID", "Color", "Player", "Role", "SubRoles");
        Players.GetPlayers().Where(p => p != null).ForEach(p =>
        {
            textTable.AddEntry((object)p.PlayerId, ModConstants.ColorNames[p.cosmetics.ColorId], p.name, p.GetCustomRole().RoleName, p.GetSubroles().Fuse());
        });
        VentLogger.Debug($"Role Assignments\n{textTable}", "RoleManager::SelectRoles~Postfix");
        Game.RenderAllForAll(state: GameState.InIntro);
    }
}