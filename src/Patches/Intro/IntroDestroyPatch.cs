using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.API.Vanilla;
using Lotus.Options;
using Lotus.Player;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using Lotus.Logging;
using Lotus.RPC;
using VentLib.Logging;
using VentLib.Networking.RPC;
using VentLib.Networking.RPC.Interfaces;
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

        DisperseAndPreventVenting();

        Async.Schedule(SetShapeshifters, NetUtils.DeriveDelay(0.15f));
        Async.Schedule(ForceApplyPets, 0.3f);
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

    // Used to circumvent anti-cheat
    private static void SetShapeshifters()
    {
        PlayerControl.AllPlayerControls.ToArray().Where(p => p != null).ForEach(p => p.RpcSetRoleDesync(RoleTypes.Shapeshifter, -3));
    }

    private static void ForceApplyPets()
    {
        Game.GetAllPlayers().ForEach(p => p.RpcSetName(p.name));
        string pet = GeneralOptions.MiscellaneousOptions.AssignedPet;
        pet = pet != "Random" ? pet : ModConstants.Pets.Values.ToList().GetRandom();
        GameData.Instance.AllPlayers.ToArray().ForEach(pi =>
        {
            if (pi?.Object == null) return;

            if (pi.Object.GetCustomRole() is not ITaskHolderRole taskHolder || !taskHolder.TasksApplyToTotal())
            {
                VentLogger.Trace($"Clearing Tasks For: {pi.Object.name}", "SyncTasks");
                pi.Tasks.Clear();
            }

            if (pi.Object.AmOwner) pi.Object.SetPet(pet);

            var outfit = pi.Outfits[PlayerOutfitType.Default];
            outfit.PetId = pet;
        });
        GeneralRPC.SendGameData();
        Game.GetAllPlayers().ForEach(p => p.CRpcShapeshift(p, false));
    }

    private static void DisperseAndPreventVenting()
    {
        if (GeneralOptions.MayhemOptions.RandomSpawn) Game.GetAllPlayers().Do(p => Game.RandomSpawn.Spawn(p));
        if (GeneralOptions.GameplayOptions.ForceNoVenting) Game.GetAlivePlayers().Where(p => !p.GetCustomRole().BaseCanVent).ForEach(VentApi.ForceNoVenting);

        bool isRandomSpawn = GeneralOptions.MayhemOptions.RandomSpawn;
        bool isForceVenting = GeneralOptions.GameplayOptions.ForceNoVenting;

        if (!isRandomSpawn && !isForceVenting) return;

        Game.GetAllPlayers().ForEach(p =>
        {
            
            if (isRandomSpawn) Game.RandomSpawn.Spawn(p);
            if (!isForceVenting || p.GetCustomRole().BaseCanVent) return;
            Async.Schedule(() => VentApi.ForceNoVenting(p), NetUtils.DeriveDelay(0.1f));
        });
    }
}