using System.Collections;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Extensions;
using Lotus.GUI.Name.Interfaces;
using Lotus.Roles;
using Lotus.Roles.Internals.Enums;
using Lotus.RPC;
using LotusTrigger.Options;
using UnityEngine;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Debug.Profiling;
using VentLib.Utilities.Extensions;
using static VentLib.Utilities.Debug.Profiling.Profilers;

namespace Lotus.Patches.Intro;


[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
class IntroDestroyPatch
{
    public static void Postfix(IntroCutscene __instance)
    {
        Profiler.Sample destroySample = Global.Sampler.Sampled();
        Game.State = GameState.Roaming;
        if (!AmongUsClient.Instance.AmHost) return;

        string pet = GeneralOptions.MiscellaneousOptions.AssignedPet;
        while (pet == "Random") pet = ModConstants.Pets.Values.ToList().GetRandom();

        Profiler.Sample fullSample = Global.Sampler.Sampled("Setup ALL Players");
        Players.GetPlayers().ForEach(p =>
        {
            Profiler.Sample executeSample = Global.Sampler.Sampled("Execution Pregame Setup");
            Async.Execute(PreGameSetup(p, pet));
            executeSample.Stop();
        });
        fullSample.Stop();

        Profiler.Sample propSample = Global.Sampler.Sampled("Propagation Sample");
        VentLogger.Trace("Intro Scene Ending", "IntroCutscene");
        ActionHandle handle = ActionHandle.NoInit();
        Game.TriggerForAll(LotusActionType.RoundStart, ref handle, true);
        propSample.Stop();

        Hooks.GameStateHooks.RoundStartHook.Propagate(new GameStateHookEvent(Game.MatchData));
        destroySample.Stop();
    }

    private static IEnumerator PreGameSetup(PlayerControl player, string pet)
    {
        if (player == null) yield break;

        FrozenPlayer frozenPlayer = new(player);
        Game.MatchData.FrozenPlayers[frozenPlayer.GameID] = frozenPlayer;

        if (player.GetVanillaRole().IsImpostor())
        {
            float cooldown = GeneralOptions.GameplayOptions.GetFirstKillCooldown(player);
            VentLogger.Trace($"Fixing First Kill Cooldown for {player.name} (Cooldown={cooldown}s)", "Fix First Kill Cooldown");
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
            VentLogger.Trace($"Clearing Tasks For: {player.name}", "SyncTasks");
            playerData.Tasks?.Clear();
        }

        bool hasPet = !(player.cosmetics?.CurrentPet?.Data?.ProductId == "pet_EmptyPet");
        if (hasPet) VentLogger.Trace($"Player: {player.name} has pet: {player.cosmetics?.CurrentPet?.Data?.ProductId}. Skipping assigning pet: {pet}.", "PetAssignment");
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
        //player.RpcProtectPlayer(player, 0); Used for server authoritive fix
    }
}