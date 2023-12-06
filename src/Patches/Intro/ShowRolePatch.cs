using HarmonyLib;
using Lotus.Roles;
using Lotus.Utilities;
using Lotus.Extensions;
using Lotus.Roles.Builtins;
using Lotus.Roles2;
using Lotus.Roles2.Definitions;
using VentLib.Utilities;

namespace Lotus.Patches.Intro;

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
class ShowRolePatch
{
    public static void Postfix(IntroCutscene __instance)
    {
        Async.Schedule(() =>
        {
            UnifiedRoleDefinition role = PlayerControl.LocalPlayer.PrimaryRole();
            if (role is NoOpDefinition) return;
            if (true)
            {
                __instance.YouAreText.color = role.RoleColor;
                __instance.RoleText.text = role.Name;
                __instance.RoleText.color = role.RoleColor;
                __instance.RoleBlurbText.color = role.RoleColor;

                __instance.RoleBlurbText.text = PlayerControl.LocalPlayer.PrimaryRole().Blurb;
            }

            __instance.RoleText.text += Utils.GetSubRolesText(PlayerControl.LocalPlayer.PlayerId);

        }, 0.01f);

    }
}