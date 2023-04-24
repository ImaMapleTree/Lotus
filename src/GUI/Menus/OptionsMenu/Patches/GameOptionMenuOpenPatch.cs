using HarmonyLib;

namespace TOHTOR.GUI.Menus.OptionsMenu.Patches;

[HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
public class GameOptionMenuOpenPatch
{
    private static CustomOptionContainer customOptionContainer;
    public static void Postfix(OptionsMenuBehaviour __instance)
    {
        customOptionContainer = __instance.gameObject.AddComponent<CustomOptionContainer>();
        customOptionContainer.PassMenu(__instance);
    }
}