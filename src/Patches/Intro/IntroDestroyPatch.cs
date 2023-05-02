using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.API.Reactive;
using TOHTOR.API.Reactive.HookEvents;
using TOHTOR.Extensions;
using TOHTOR.Options;
using TOHTOR.Player;
using TOHTOR.Roles.Extra;
using TOHTOR.Roles.Internals;
using TOHTOR.RPC;
using VentLib.Logging;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Patches.Intro;


[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
class IntroDestroyPatch
{
    public static void Postfix(IntroCutscene __instance)
    {
        Game.State = GameState.Roaming;
        if (!GameStates.IsInGame) return;
        if (!AmongUsClient.Instance.AmHost) return;

        if (TOHPlugin.NormalOptions.MapId != 4)
        {
            PlayerControl.AllPlayerControls.ToArray().Do(pc => pc.RpcResetAbilityCooldown());
            if (GeneralOptions.GameplayOptions.FixFirstKillCooldown) Async.Schedule(FixFirstKillCooldown, 2f);
        }

        if (PlayerControl.LocalPlayer.GetCustomRole() is GM) PlayerControl.LocalPlayer.RpcExileV2();

        if (GeneralOptions.MayhemOptions.RandomSpawn) Game.GetAllPlayers().Do(p => Game.RandomSpawn.Spawn(p));

        Async.Schedule(SetShapeshifters, NetUtils.DeriveDelay(0.15f));
        Async.Schedule(ForceApplyPets, 0.3f);
        Async.Schedule(() => Game.RenderAllForAll(force: true), NetUtils.DeriveDelay(0.6f));
        Game.GetAllPlayers().Select(p => new FrozenPlayer(p)).ForEach(p => Game.GameHistory.FrozenPlayers[p.GameID] = p);

        VentLogger.Trace("Intro Scene Ending", "IntroCutscene");
        Hooks.GameStateHooks.RoundStartHook.Propagate(new GameStateHookEvent());
    }

    private static void FixFirstKillCooldown()
    {
        PlayerControl.AllPlayerControls.ToArray()
            .Where(p => p != null)
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

            if (pi.Object.AmOwner) {
                pi.Object.SetPet(pet);
                return;
            }

            var outfit = pi.Outfits[PlayerOutfitType.Default];
            outfit.PetId = pet;
        });
        GeneralRPC.SendGameData();
        Game.GetAllPlayers().ForEach(p => p.CRpcShapeshift(p, false));
    }
}