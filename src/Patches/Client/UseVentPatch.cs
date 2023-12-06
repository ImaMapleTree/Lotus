using HarmonyLib;
using Lotus.Roles;
using Lotus.Extensions;
using Lotus.Roles2;

namespace Lotus.Patches.Client;

[HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
class UseVentPatch
{
    public static bool Prefix(Vent __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] ref bool canUse)
    {
        if (pc.Object == null) return true;
        UnifiedRoleDefinition role = pc.Object.PrimaryRole();

        if (role.CanVent()) return true;
        return canUse = false;
    }
}