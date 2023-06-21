using HarmonyLib;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.GUI.Menus.OptionsMenu.Patches;

[HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
public class GameOptionMenuOpenPatch
{
    private static CustomOptionContainer customOptionContainer;
    public static OptionsMenuBehaviour MenuBehaviour;
    public static void Postfix(OptionsMenuBehaviour __instance)
    {
        MenuBehaviour = __instance;
        customOptionContainer = __instance.gameObject.AddComponent<CustomOptionContainer>();
        customOptionContainer.PassMenu(__instance);
    }

    [QuickPrefix(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.ResetText))]
    public static bool DisableResetTextFunc(OptionsMenuBehaviour __instance) => false;
}