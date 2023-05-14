using HarmonyLib;
using Lotus.Roles;
using Lotus.Extensions;

namespace Lotus.Patches.Client;

[HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
class UseVentPatch
{
    public static bool Prefix(Vent __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] ref bool canUse)
    {
        if (pc.Object == null) return true;
        CustomRole role = pc.Object.GetCustomRole();

        if (role.BaseCanVent) return true;
        return canUse = false;
    }
}