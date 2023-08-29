using System.Collections.Generic;
using HarmonyLib;
using Lotus.API.Reactive;
using Lotus.Server;
using VentLib.Utilities.Extensions;

namespace Lotus.Patches.Actions;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RemoveProtection))]
public class RemoveProtectionPatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(RemoveProtectionPatch));
    public static readonly Dictionary<byte, List<byte>> InteractionIgnoredMapping = new();

    static RemoveProtectionPatch()
    {
        Hooks.GameStateHooks.GameStartHook.Bind(nameof(RemoveProtectionPatch), () => InteractionIgnoredMapping.Clear());
    }

    public static void AddIgnoredInteraction(byte source, byte target = 255)
    {
        if (target == 255) target = source;
        InteractionIgnoredMapping.GetOrCompute(source, () => new List<byte>()).Add(target);
    }

    public static void Postfix(PlayerControl __instance)
    {
        if (MurderPatches.LastAttacker != null)
        {
            List<byte> ignoredTargets = InteractionIgnoredMapping.GetOrCompute(MurderPatches.LastAttacker.PlayerId, () => new List<byte>());
            int byteIndex = ignoredTargets.IndexOf(__instance.PlayerId);
            if (byteIndex != -1)
            {
                log.Trace($"Ignoring last interaction by {MurderPatches.LastAttacker.name} against {__instance.name}.");
                ignoredTargets.RemoveAt(byteIndex);
                return;
            }
        }

        ServerPatchManager.Patch.Execute(PatchedCode.RemoveProtection, __instance);
    }
}