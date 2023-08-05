using HarmonyLib;
using Lotus.Extensions;

namespace Lotus.Patches.Actions;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ProtectPlayer))]
class ProtectPlayerPatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(ProtectPlayerPatch));

    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        log.Info($"{__instance.GetNameWithRole()} => {target.GetNameWithRole()}", "ProtectPlayer");
    }
}