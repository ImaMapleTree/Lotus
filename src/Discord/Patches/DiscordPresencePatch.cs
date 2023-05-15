using Discord;
using HarmonyLib;
using Lotus.Logging;

namespace Lotus.Discord.Patches;

[HarmonyPatch(typeof(ActivityManager), nameof(ActivityManager.UpdateActivity))]
public class DiscordPresencePatch
{
    public static void Prefix(ActivityManager __instance, ref Activity activity)
    {
        DevLogger.Log("Discord Presence");
        activity.Details = $"Project Lotus {ProjectLotus.Instance.CurrentVersion.ToSimpleName()}";
    }
}
