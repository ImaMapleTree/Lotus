using AmongUs.GameOptions;
using HarmonyLib;
using TOHTOR.Extensions;
using TOHTOR.Roles.RoleGroups.Crew;
using TOHTOR.Roles.RoleGroups.NeutralKilling;

namespace TOHTOR.Patches.Client;

[HarmonyPatch(typeof(ConsoleJoystick), nameof(ConsoleJoystick.HandleHUD))]
class ConsoleJoystickHandleHUDPatch
{
    public static void Postfix()
    {
        HandleHUDPatch.Postfix(ConsoleJoystick.player);
    }
}
[HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.HandleHud))]
class KeyboardJoystickHandleHUDPatch
{
    public static void Postfix()
    {
        HandleHUDPatch.Postfix(KeyboardJoystick.player);
    }
}

class HandleHUDPatch
{
    public static void Postfix(Rewired.Player player)
    {
        if (player.GetButtonDown(8) && // 8:キルボタンのactionId
            PlayerControl.LocalPlayer.Data?.Role?.IsImpostor == false &&
            (PlayerControl.LocalPlayer.GetCustomRole() is Sheriff or Arsonist or Jackal) && PlayerControl.LocalPlayer.Data.Role.Role != RoleTypes.GuardianAngel)
        {
            DestroyableSingleton<HudManager>.Instance.KillButton.DoClick();
        }
        if (player.GetButtonDown(50) && !PlayerControl.LocalPlayer.GetCustomRole().CanVent())
        {
            DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.DoClick();
        }
    }
}