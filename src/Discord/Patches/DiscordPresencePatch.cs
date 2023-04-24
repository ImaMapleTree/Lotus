using Discord;
using HarmonyLib;

namespace TOHTOR.Discord.Patches;

[HarmonyPatch(typeof(ActivityManager), nameof(ActivityManager.UpdateActivity))]
public class DiscordPresencePatch
{
    public static void Prefix(ActivityManager __instance, ref Activity activity)
    {
    }
}
