namespace Lotus.Patches.Systems;

// This is not a very fun patch
/*[HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
class CanUsePatch
{
    public static bool Prefix(ref float __result, Console __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
    {
        canUse = couldUse = false;
        //こいつをfalseでreturnしても、タスク(サボ含む)以外の使用可能な物は使えるまま(ボタンなど)
        return __instance.AllowImpostor || Utils.HasTasks(PlayerControl.LocalPlayer.Data);
    }
}*/