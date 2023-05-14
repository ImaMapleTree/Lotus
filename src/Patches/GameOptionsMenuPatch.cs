using HarmonyLib;

namespace Lotus.Patches;

// This patch allows host to have bigger range when setting options
[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
public class GameOptionsMenuPatch
{
    public static void Postfix(GameOptionsMenu __instance)
    {
        foreach (var ob in __instance.Children)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (ob.Title)
            {
                case StringNames.GameShortTasks:
                case StringNames.GameLongTasks:
                case StringNames.GameCommonTasks:
                    ob.Cast<NumberOption>().ValidRange = new FloatRange(0, 99);
                    break;
                case StringNames.GameKillCooldown:
                    ob.Cast<NumberOption>().ValidRange = new FloatRange(0, 180);
                    break;
            }
        }
    }
}