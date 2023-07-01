using AmongUs.GameOptions;
using HarmonyLib;
using Lotus.Extensions;
using VentLib.Logging;
using VentLib.Networking.RPC;
using VentLib.Utilities;

namespace Lotus.Patches;

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRoleOnDeath))]
public class AssignRoleOnDeathPatch
{
    public static bool Prefix(RoleManager __instance, [HarmonyArgument(0)] PlayerControl player)
    {
        return false;
    }

    public static void Postfix(RoleManager __instance, [HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] bool specialRolesAllowed)
    {
        RpcV3.Immediate(player.NetId, RpcCalls.SetRole)
            .Write((ushort)(player.GetVanillaRole().IsImpostor() ? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost))
            .Send(player.GetClientId());
        player.Data.DefaultOutfit.PetId = "pet_EmptyPet";
        VentLogger.Debug($"Dead Player {player.name} => {player.Data.Role.Role}");
    }
}