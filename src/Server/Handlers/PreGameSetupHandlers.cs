using System.Collections;
using AmongUs.GameOptions;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Extensions;
using Lotus.GUI.Name.Interfaces;
using Lotus.Roles;
using Lotus.Roles.Interfaces;
using Lotus.RPC;
using Lotus.Server.Interfaces;
using LotusTrigger.Options;
using UnityEngine;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Server.Handlers;

internal class PreGameSetupHandlers
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(PreGameSetupHandlers));

    public static IPreGameSetupHandler StandardHandler = new Standard();
    public static IPreGameSetupHandler ProtectionPatchedHandler = new ProtectionPatched();

    private class Standard : IPreGameSetupHandler
    {
        public virtual IEnumerator PreGameSetup(PlayerControl player, string pet)
        {
            if (player == null) yield break;

            FrozenPlayer frozenPlayer = new(player);
            Game.MatchData.FrozenPlayers[frozenPlayer.GameID] = frozenPlayer;

            if (player.GetVanillaRole().IsImpostor())
            {
                float cooldown = GeneralOptions.GameplayOptions.GetFirstKillCooldown(player);
                log.Trace($"Fixing First Kill Cooldown for {player.name} (Cooldown={cooldown}s)", "Fix First Kill Cooldown");
                player.SetKillCooldown(cooldown);
                player.Data.Role.SetCooldown();
            }

            if (GeneralOptions.MayhemOptions.RandomSpawn) Game.RandomSpawn.Spawn(player);

            player.RpcSetRoleDesync(RoleTypes.Shapeshifter, -3);
            yield return new WaitForSeconds(0.15f);
            if (player == null) yield break;

            GameData.PlayerInfo playerData = player.Data;
            if (playerData == null) yield break;

            CustomRole role = player.GetCustomRole();
            if (role is not ITaskHolderRole taskHolder || !taskHolder.TasksApplyToTotal())
            {
                log.Trace($"Clearing Tasks For: {player.name}", "SyncTasks");
                playerData.Tasks?.Clear();
            }

            bool hasPet = !(player.cosmetics?.CurrentPet?.Data?.ProductId == "pet_EmptyPet");
            if (hasPet) log.Trace($"Player: {player.name} has pet: {player.cosmetics?.CurrentPet?.Data?.ProductId}. Skipping assigning pet: {pet}.", "PetAssignment");
            else if (player.AmOwner) player.SetPet(pet);
            else playerData.DefaultOutfit.PetId = pet;
            playerData.PlayerName = player.name;

            Players.SendPlayerData(playerData, autoSetName: false);
            yield return new WaitForSeconds(NetUtils.DeriveDelay(0.05f));
            if (player == null) yield break;

            if (!hasPet) player.CRpcShapeshift(player, false);

            INameModel nameModel = player.NameModel();
            Players.GetPlayers().ForEach(p => nameModel.RenderFor(p, force: true));
            player.SyncAll();
        }
    }

    private class ProtectionPatched : Standard
    {
        public override IEnumerator PreGameSetup(PlayerControl player, string pet)
        {
            IEnumerator enumerator = base.PreGameSetup(player, pet);
            yield return enumerator;
            log.Info($"Protecting player {player.name}. (Server Patch)");
            player.RpcProtectPlayer(player, 0);
        }
    }
}