using HarmonyLib;
using Lotus.Logging;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.GUI.Menus.OptionsMenu.Patches;

[HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
public class GameOptionMenuOpenPatch
{
    private static CustomOptionContainer customOptionContainer;
    public static void Postfix(OptionsMenuBehaviour __instance)
    {
        DevLogger.Log("Starting in here");
        customOptionContainer = __instance.gameObject.AddComponent<CustomOptionContainer>();
        customOptionContainer.PassMenu(__instance);

        DevLogger.Log("finishing here");
    }

    [QuickPrefix(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.ResetText))]
    public static bool DisableResetTextFunc(OptionsMenuBehaviour __instance) => false;
}