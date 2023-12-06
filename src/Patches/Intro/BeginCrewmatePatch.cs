using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Lotus.API;
using Lotus.Extensions;
using Lotus.Logging;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles2;
using Lotus.Roles2.Definitions;
using UnityEngine;
using VentLib.Localization;

namespace Lotus.Patches.Intro;

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
class BeginCrewmatePatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(BeginCrewmatePatch));

    public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
    {
        //チーム表示変更
        DevLogger.Log("Begin Crewmate");
        UnifiedRoleDefinition role = PlayerControl.LocalPlayer.PrimaryRole();
        if (role is NoOpDefinition) return;

        switch (role.Metadata.GetOrDefault(LotusKeys.AuxiliaryRoleType, SpecialType.None))
        {
            case SpecialType.NeutralKilling:
            case SpecialType.Undead:
            case SpecialType.Neutral:
                __instance.TeamTitle.text = Factions.FactionInstances.Neutral.Name();
                __instance.TeamTitle.color = Color.white;
                __instance.ImpostorText.gameObject.SetActive(true);
                __instance.ImpostorText.text = "";
                __instance.BackgroundBar.material.color = role.RoleColor;
                break;
            case SpecialType.Madmate:
                __instance.TeamTitle.text = Localizer.Translate("Roles.Madmate.RoleName");
                __instance.TeamTitle.color = ModConstants.Palette.MadmateColor;
                __instance.ImpostorText.text = "";
                StartFadeIntro(__instance, Palette.CrewmateBlue, Palette.ImpostorRed);
                PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Impostor);
                break;
        }

        if (role.RoleDefinition is not GameMaster) return;

        __instance.TeamTitle.text = role.Name;
        __instance.TeamTitle.color = role.RoleColor;
        __instance.BackgroundBar.material.color = role.RoleColor;
        __instance.ImpostorText.gameObject.SetActive(false);
    }

    private static AudioClip? GetIntroSound(RoleTypes roleType)
    {
        return RoleManager.Instance.AllRoles.FirstOrDefault(role => role.Role == roleType)?.IntroSound;
    }

    private static async void StartFadeIntro(IntroCutscene __instance, Color start, Color end)
    {
        await System.Threading.Tasks.Task.Delay(1000);
        int milliseconds = 0;
        while (true)
        {
            DevLogger.Log("???");
            await System.Threading.Tasks.Task.Delay(20);
            milliseconds += 20;
            float time = milliseconds / (float)500;
            Color lerpingColor = Color.Lerp(start, end, time);
            if (__instance == null || milliseconds > 500)
            {
                log.Trace("Exit The Loop (GTranslated)", "StartFadeIntro");
                break;
            }
            __instance.BackgroundBar.material.color = lerpingColor;
        }
    }
}