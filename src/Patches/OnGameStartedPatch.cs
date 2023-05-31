using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.Gamemodes;
using Lotus.Managers;
using Lotus.Roles.RoleGroups.NeutralKilling;
using Lotus.Extensions;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Patches
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    class ChangeRoleSettings
    {
        public static void Prefix(AmongUsClient __instance)
        {

            Game.Setup();
            /*GameOptionsManager.Instance.CurrentGameOptions = GameOptionsManager.Instance.normalGameHostOptions.Cast<IGameOptions>();*/
        }

        public static void Postfix(AmongUsClient __instance)
        {
            ProjectLotus.ResetCamPlayerList = new List<byte>();
            FallFromLadder.Reset();

            Game.State = GameState.InIntro;
            Game.GetAllPlayers().Do(p => Game.MatchData.Roles.MainRoles[p.PlayerId] = CustomRoleManager.Default);
            Game.CurrentGamemode.Setup();
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    class SelectRolesPatch
    {
        public static void Prefix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            Game.CurrentGamemode.AssignRoles(Game.GetAllPlayers().ToList());
        }

        public static void Postfix()
        {
            if (!AmongUsClient.Instance.AmHost) return;

            Game.GetAllPlayers().Do(p => p.GetCustomRole().SyncOptions());

            TextTable textTable = new("ID", "Color", "Player", "Role", "SubRoles");
            Game.GetAllPlayers().Where(p => p != null).ForEach(p =>
            {
                textTable.AddEntry((object)p.PlayerId, ModConstants.ColorNames[p.cosmetics.ColorId], p.name, p.GetCustomRole().RoleName, p.GetSubroles().Fuse());
            });
            VentLogger.Debug($"Role Assignments\n{textTable}", "RoleManager::SelectRoles~Postfix");
            

            ProjectLotus.ResetCamPlayerList.AddRange(Game.GetAllPlayers().Where(p => p.GetCustomRole() is Arsonist).Select(p => p.PlayerId));
            Game.RenderAllForAll(state: GameState.InIntro);
            Game.CurrentGamemode.Trigger(GameAction.GameStart);
        }
    }
}