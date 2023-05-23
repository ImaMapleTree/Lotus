using System.Collections;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Options;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using Lotus.Roles;
using Lotus.RPC;
using UnityEngine;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Patches.Intro;


[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
class IntroDestroyPatch
{
    public static void Postfix(IntroCutscene __instance)
    {
        Game.State = GameState.Roaming;
        if (!GameStates.IsInGame) return;
        if (!AmongUsClient.Instance.AmHost) return;

        if (ProjectLotus.NormalOptions.MapId != 4)
        {
            PlayerControl.AllPlayerControls.ToArray().Do(pc => pc.RpcResetAbilityCooldown());
            if (GeneralOptions.GameplayOptions.FixFirstKillCooldown) FixFirstKillCooldown();
        }

        if (GeneralOptions.MayhemOptions.RandomSpawn) Game.GetAllPlayers().Do(p => Game.RandomSpawn.Spawn(p));

        string pet = GeneralOptions.MiscellaneousOptions.AssignedPet;
        pet = pet != "Random" ? pet : ModConstants.Pets.Values.ToList().GetRandom();
        
        Async.Schedule(() => Game.GetAllPlayers().ForEach(p => Async.Execute(PreGameSetup(p, pet))), NetUtils.DeriveDelay(0.05f));
        Async.Schedule(() => Game.RenderAllForAll(force: true), NetUtils.DeriveDelay(0.6f));
        
        Game.GetAllPlayers().Select(p => new FrozenPlayer(p)).ForEach(p => Game.MatchData.FrozenPlayers[p.GameID] = p);

        VentLogger.Trace("Intro Scene Ending", "IntroCutscene");
        ActionHandle handle = ActionHandle.NoInit();
        Game.TriggerForAll(RoleActionType.RoundStart, ref handle, true);

        Hooks.GameStateHooks.RoundStartHook.Propagate(new GameStateHookEvent(Game.MatchData));
        Game.SyncAll();
    }

    private static void FixFirstKillCooldown()
    {
        VentLogger.Trace("Fixing First Kill Cooldown", "FixFirstKillCooldown");
        Game.GetAllPlayers().Where(p => p != null)
            .Where(p => p.GetVanillaRole().IsImpostor())
            .ForEach(p => p.SetKillCooldown(p.GetCustomRole().GetOverride(Override.KillCooldown)?.GetValue() as float? ?? AUSettings.KillCooldown()));
    }

    private static IEnumerator PreGameSetup(PlayerControl player, string pet)
    {
        if (player == null) yield break;
        player.RpcSetRoleDesync(RoleTypes.Shapeshifter, -3);
        yield return new WaitForSeconds(0.15f);
        if (player == null) yield break;
        
        GameData.PlayerInfo playerData = player.Data;
        if (playerData == null) yield break;

        CustomRole role = player.GetCustomRole();
        if (role is not ITaskHolderRole taskHolder || !taskHolder.TasksApplyToTotal())
        {
            VentLogger.Trace($"Clearing Tasks For: {player.name}", "SyncTasks");
            playerData.Tasks.Clear();
        }

        var outfit = playerData.Outfits[PlayerOutfitType.Default];

        if (player.AmOwner) player.SetPet(pet);

        outfit.PetId = pet;
        Players.SendPlayerData(playerData, player.GetClientId());
        yield return new WaitForSeconds(NetUtils.DeriveDelay(0.05f));
        if (player == null) yield break;
        player.CRpcShapeshift(player, false);
    }
}