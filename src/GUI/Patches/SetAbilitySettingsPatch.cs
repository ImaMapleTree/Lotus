using System.Linq;
using HarmonyLib;
using Lotus.Extensions;
using Lotus.Roles2.GUI;
using VentLib.Utilities.Extensions;

namespace Lotus.GUI.Patches;

[HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.SetFromSettings))]
public class SetAbilitySettingsPatch
{
    public static bool Prefix(AbilityButton __instance)
    {
        PlayerControl.LocalPlayer.GetAllRoleDefinitions().ForEach(rd => rd.RoleDefinition.GUIProvider.ForceUpdate());
        return !PlayerControl.LocalPlayer.GetAllRoleDefinitions().Any(rd =>
        {
            GUIProvider provider = rd.RoleDefinition.GUIProvider;
            return !provider.Initialized || provider.AbilityButton.IsOverriding;
        });
    }
}