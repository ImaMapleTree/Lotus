using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using TOHTOR.Extensions;
using TOHTOR.Factions.Crew;
using TOHTOR.Managers;
using TOHTOR.Roles;
using TOHTOR.Roles.Extra;
using TOHTOR.Roles.Legacy;
using TOHTOR.Roles.RoleGroups.Crew;
using TOHTOR.Roles.RoleGroups.Impostors;
using TOHTOR.Roles.RoleGroups.Neutral;
using TOHTOR.Roles.RoleGroups.NeutralKilling;
using TOHTOR.Utilities;
using UnityEngine;
using VentLib.Localization;
using VentLib.Logging;

namespace TOHTOR.Patches.Intro;

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
class BeginCrewmatePatch
{
    public static void Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
    {
        if (PlayerControl.LocalPlayer.GetCustomRole().Faction is Crewmates) return;

        var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
        soloTeam.Add(PlayerControl.LocalPlayer);
        teamToDisplay = soloTeam;
    }


    public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
    {
        //チーム表示変更
        CustomRole role = PlayerControl.LocalPlayer.GetCustomRole();
        RoleType roleType = role.GetRoleType();

        switch (roleType)
        {
            case RoleType.Neutral:
                __instance.TeamTitle.text = role.RoleName;
                __instance.TeamTitle.color = role.RoleColor;
                __instance.ImpostorText.gameObject.SetActive(true);
                __instance.ImpostorText.text = role switch
                {
                    Egoist => Localizer.Translate("Roles.Egoist.TeamEgoist"),
                    Jackal => Localizer.Translate("Roles.Jackal.TeamJackal"),
                    _ => Localizer.Translate("Roles.Miscellaneous.NeutralText"),
                };
                __instance.BackgroundBar.material.color = role.RoleColor;
                break;
            case RoleType.Madmate:
                __instance.TeamTitle.text = Localizer.Translate("Roles.Madmate.RoleName");
                __instance.TeamTitle.color = CustomRoleManager.Static.Madmate.RoleColor;
                __instance.ImpostorText.text = Localizer.Translate("Roles.Miscellaneous.ImpostorText");
                StartFadeIntro(__instance, Palette.CrewmateBlue, Palette.ImpostorRed);
                PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Impostor);
                break;
        }
        switch (role)
        {
            case Terrorist:
                var sound = ShipStatus.Instance.CommonTasks.FirstOrDefault(task => task.TaskType == TaskTypes.FixWiring)?.MinigamePrefab.OpenSound;
                PlayerControl.LocalPlayer.Data.Role.IntroSound = sound;
                break;

            case Executioner:
                PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Shapeshifter);
                break;

            case Vampire:
                PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Shapeshifter);
                break;

            case SabotageMaster:
                PlayerControl.LocalPlayer.Data.Role.IntroSound = ShipStatus.Instance.SabotageSound;
                break;

            case Sheriff:
                PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Crewmate);
                __instance.BackgroundBar.material.color = Palette.CrewmateBlue;
                break;
            case Arsonist:
                PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Crewmate);
                break;

            case Copycat:
                PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Impostor);
                break;

            case Mayor:
                PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Crewmate);
                break;

            case GM:
                __instance.TeamTitle.text = role.RoleName;
                __instance.TeamTitle.color = role.RoleColor;
                __instance.BackgroundBar.material.color = role.RoleColor;
                __instance.ImpostorText.gameObject.SetActive(false);
                break;

        }

        if (Input.GetKey(KeyCode.RightShift))
        {
            __instance.TeamTitle.text = "Town Of Host:\nThe Other Roles";
            __instance.ImpostorText.gameObject.SetActive(true);
            __instance.ImpostorText.text = "https://github.com/music-discussion/TOHTOR-TheOtherRoles--TOH-TOR" +
                                           "\r\nv0.9.4 - Out Now on Github";
            __instance.TeamTitle.color = Utils.ConvertHexToColor("#73fa73");
            StartFadeIntro(__instance, Color.cyan, Color.yellow);
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            __instance.TeamTitle.text = "Town Of Host:\nThe Other Roles";
            __instance.ImpostorText.gameObject.SetActive(true);
            __instance.ImpostorText.text = "https://github.com/music-discussion/TOHTOR-TheOtherRoles--TOH-TOR" +
                                           "\r\nv0.9.4 - Coming Soon on Github";
            __instance.TeamTitle.color = Utils.ConvertHexToColor("#73fa73");
            StartFadeIntro(__instance, Color.cyan, Color.yellow);
        }
        if (Input.GetKey(KeyCode.RightControl))
        {
            __instance.TeamTitle.text = "Discord Server";
            __instance.ImpostorText.gameObject.SetActive(true);
            __instance.ImpostorText.text = "https://discord.gg/tohtor";
            __instance.TeamTitle.color = Utils.ConvertHexToColor("#73fa73");
            StartFadeIntro(__instance, Utils.ConvertHexToColor("#73fa73"), Utils.ConvertHexToColor("#73fa73"));
        }
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
            await System.Threading.Tasks.Task.Delay(20);
            milliseconds += 20;
            float time = milliseconds / (float)500;
            Color lerpingColor = Color.Lerp(start, end, time);
            if (__instance == null || milliseconds > 500)
            {
                VentLogger.Trace("Exit The Loop (GTranslated)", "StartFadeIntro");
                break;
            }
            __instance.BackgroundBar.material.color = lerpingColor;
        }
    }
}