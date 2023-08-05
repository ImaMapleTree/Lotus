using HarmonyLib;
using Lotus.Server;

namespace Lotus.Patches.Actions;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RemoveProtection))]
public class RemoveProtectionPatch
{
    public static void Postfix(PlayerControl __instance) => ServerPatchManager.Patch.Execute(PatchedCode.RemoveProtection, __instance);
}