using System;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Options;
using TOHTOR.Roles;
using TOHTOR.Roles.Legacy;
using TOHTOR.Roles.RoleGroups.Crew;
using TOHTOR.Roles.RoleGroups.Impostors;
using TOHTOR.Roles.RoleGroups.Madmates.Roles;
using TOHTOR.Roles.RoleGroups.Neutral;
using TOHTOR.Roles.RoleGroups.NeutralKilling;
using UnityEngine;
using VentLib.Localization;
using VentLib.Logging;
using VentLib.Utilities;
using Impostor = TOHTOR.Roles.RoleGroups.Vanilla.Impostor;

namespace TOHTOR.GUI.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
class HudManagerPatch
{
    public static bool ShowDebugText = false;
    public static int LastCallNotifyRolesPerSecond = 0;
    public static int NowCallNotifyRolesCount = 0;
    public static int LastSetNameDesyncCount = 0;
    public static int LastFPS = 0;
    public static int NowFrameCount = 0;
    public static float FrameRateTimer = 0.0f;
    public static TMPro.TextMeshPro LowerInfoText;

    public static void Postfix(HudManager __instance)
    {
        var player = PlayerControl.LocalPlayer;
        if (player == null) return;
        var TaskTextPrefix = "";
        var FakeTasksText = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FakeTasks, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
        //壁抜け
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if ((!AmongUsClient.Instance.IsGameStarted || !GameStates.IsOnlineGame)
                && player.CanMove)
            {
                player.Collider.offset = new Vector2(0f, 127f);
            }
        }
        //壁抜け解除
        if (player.Collider.offset.y == 127f)
        {
            if (!Input.GetKey(KeyCode.LeftControl) || (AmongUsClient.Instance.IsGameStarted && GameStates.IsOnlineGame))
            {
                player.Collider.offset = new Vector2(0f, -0.3636f);
            }
        }


        __instance.GameSettings.text = OptionShower.GetOptionShower().GetPage();
        //ゲーム中でなければ以下は実行されない
        if (!AmongUsClient.Instance.IsGameStarted) return;


        if (SetHudActivePatch.IsActive)
        {//MOD入り用のボタン下テキスト変更
            switch (player.GetCustomRole())
            {
                /*case Sniper:
                    __instance.AbilityButton.OverrideText(SniperOLD.OverrideShapeText(player.PlayerId));
                    break;*/
                case FireWorker:
                    __instance.AbilityButton.OverrideText($"{Localizer.Translate("Roles.FireWorks.AbilityText")}");
                    break;
                /*case SerialKiller:
                    // ? What ?
                    SerialKillerOLD.GetAbilityButtonText(__instance, player);
                    break;*/
                case Warlock warlock:
                    __instance.KillButton.OverrideText(Localizer.Translate(!warlock.Shapeshifted
                        ? "Roles.Warlock.CurseButtonText"
                        : Localizer.Translate("Roles.Warlock.KillButtonText")));
                    break;
                /*case Witch:
                    WitchOLD.GetAbilityButtonText(__instance);
                    break;*/
                case Vampire:
                    __instance.KillButton.OverrideText($"{Localizer.Translate("Roles.Vampire.KillButtonText")}");
                    break;
                case Arsonist:
                    __instance.KillButton.OverrideText($"{Localizer.Translate("Roles.Arsonist.KillButtonText")}");
                    break;
                case Puppeteer:
                    __instance.KillButton.OverrideText($"{Localizer.Translate("Roles.Puppeteer.KillButtonText")}");
                    break;
                /*case BountyHunter:
                    BountyHunterOLD.GetAbilityButtonText(__instance);
                    break;
                case EvilTracker:
                    EvilTrackerOLD.GetAbilityButtonText(__instance, player.PlayerId);
                    break;*/
            }

            //バウンティハンターのターゲットテキスト
            if (LowerInfoText == null)
            {
                LowerInfoText = UnityEngine.Object.Instantiate(__instance.KillButton.buttonLabelText);
                LowerInfoText.transform.parent = __instance.transform;
                LowerInfoText.transform.localPosition = new Vector3(0, -2f, 0);
                LowerInfoText.alignment = TMPro.TextAlignmentOptions.Center;
                LowerInfoText.overflowMode = TMPro.TextOverflowModes.Overflow;
                LowerInfoText.enableWordWrapping = false;
                LowerInfoText.color = Palette.EnabledColor;
                LowerInfoText.fontSizeMin = 2.0f;
                LowerInfoText.fontSizeMax = 2.0f;
            }


            LowerInfoText.enabled = false;
            if (!AmongUsClient.Instance.IsGameStarted && AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay)
            {
                LowerInfoText.enabled = false;
            }

            if (player.CanUseKillButton())
            {
                __instance.KillButton.ToggleVisible(!player.Data.IsDead);
            }
            else
            {
                __instance.KillButton.SetDisabled();
                __instance.KillButton.ToggleVisible(false);
            }
            switch (player.GetCustomRole())
            {
                case Madmate:
                case Jester:
                    TaskTextPrefix += FakeTasksText;
                    break;
                case Sheriff:
                case Arsonist:
                case Jackal:
                    player.CanUseImpostorVent();
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        player.Data.Role.CanUseKillButton = true;
                    break;
            }
        }

        if (!Input.GetKeyDown(KeyCode.Y) || AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay) return;

        __instance.ToggleMapVisible(new MapOptions()
        {
            Mode = MapOptions.Modes.Sabotage,
            AllowMovementWhileMapOpen = true
        });

        if (!player.AmOwner) return;

        player.MyPhysics.inputHandler.enabled = true;
        ConsoleJoystick.SetMode_Task();
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ToggleHighlight))]
class ToggleHighlightPatch
{
    private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");

    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] bool active, [HarmonyArgument(1)] RoleTeamTypes team)
    {
        var player = PlayerControl.LocalPlayer;
        if (player.Data.IsDead) return;
        __instance.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", player.GetCustomRole().RoleColor);
    }
}
[HarmonyPatch(typeof(Vent), nameof(Vent.SetOutline))]
class SetVentOutlinePatch
{
    private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
    private static readonly int AddColor = Shader.PropertyToID("_AddColor");

    public static void Postfix(Vent __instance, [HarmonyArgument(1)] ref bool mainTarget)
    {
        CustomRole role = PlayerControl.LocalPlayer.GetCustomRole();

        Color color = !PlayerControl.LocalPlayer.IsAlive() || !role.CanVent() ? Color.clear : role.RoleColor;

        __instance.myRend.material.SetColor(OutlineColor, color);
        __instance.myRend.material.SetColor(AddColor, mainTarget ? color : Color.clear);
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive), typeof(bool))]
class SetHudActivePatch
{
    public static bool IsActive;
    public static void Postfix(HudManager __instance, [HarmonyArgument(0)] bool isActive)
    {
        var player = PlayerControl.LocalPlayer;

        switch (player.GetCustomRole())
        {
            case Impostor impostor:
                if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                    __instance.KillButton.ToggleVisible(!player.Data.IsDead);
                __instance.SabotageButton.ToggleVisible(impostor.CanSabotage());
                __instance.ImpostorVentButton.ToggleVisible(impostor.CanVent());
                // __instance.AbilityButton.ToggleVisible(true);
                break;
            case Sheriff sheriff:
                if (sheriff.DesyncRole == null) return;
                if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                    __instance.KillButton.ToggleVisible(isActive && !player.Data.IsDead);
                __instance.SabotageButton.ToggleVisible(false);
                __instance.ImpostorVentButton.ToggleVisible(false);
                __instance.AbilityButton.ToggleVisible(false);
                break;
        }
    }
}

[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
class ShowNormalMapPatch
{
    public static void Prefix(ref RoleTeamTypes __state)
    {
        var player = PlayerControl.LocalPlayer;
        if (player.GetCustomRole() is not (Sheriff or Arsonist)) return;

        __state = player.Data.Role.TeamType;
        player.Data.Role.TeamType = RoleTeamTypes.Crewmate;
    }

    public static void Postfix(ref RoleTeamTypes __state)
    {
        var player = PlayerControl.LocalPlayer;
        if (player.GetCustomRole() is not (Sheriff or Arsonist)) return;
        player.Data.Role.TeamType = __state;
    }
}

[HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.SetTaskText))]
class TaskPanelBehaviourPatch
{
    // タスク表示の文章が更新・適用された後に実行される
    public static void Postfix(TaskPanelBehaviour __instance)
    {
        PlayerControl player = PlayerControl.LocalPlayer;
        CustomRole role = player.GetCustomRole();
        if (role.IsVanilla()) return;

        string modifiedText = __instance.taskText.text;
        int impostorTaskIndex = modifiedText.IndexOf(":</color>", StringComparison.Ordinal);
        if (impostorTaskIndex != -1) modifiedText = modifiedText[(9 + impostorTaskIndex)..];
        string roleWithInfo = $"{role.RoleName}:\r\n";
        roleWithInfo += role.Blurb + (role.RealRole.IsImpostor() ? "" : "\r\n");
        __instance.taskText.text = role.RoleColor.Colorize(roleWithInfo) + modifiedText;
    }
}