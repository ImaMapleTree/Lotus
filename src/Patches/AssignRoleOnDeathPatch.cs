using AmongUs.GameOptions;
using HarmonyLib;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Extensions;
using VentLib.Logging;

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
        player.RpcSetRole(player.GetVanillaRole().IsImpostor() ? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost);
        VentLogger.Debug($"Dead Player {player.name} => {player.Data.Role.Role}");
    }
}