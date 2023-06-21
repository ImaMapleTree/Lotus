using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Gamemodes;
using Lotus.Managers;
using Lotus.Roles.RoleGroups.NeutralKilling;
using Lotus.Extensions;
using Lotus.Options;
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
            MatchData.AssignRole(PlayerControl.LocalPlayer, CustomRoleManager.Special.GM, true);
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


        ProjectLotus.ResetCamPlayerList.AddRange(Players.GetPlayers().Where(p => p.GetCustomRole() is Arsonist).Select(p => p.PlayerId));
        Game.RenderAllForAll(state: GameState.InIntro);
        Game.CurrentGamemode.Trigger(GameAction.GameStart);
    }
}