namespace Lotus.Patches.Hud;

//TODO Rethink for fairness
/*[HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive), typeof(bool))]
class SetHudActivePatch
{
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
}*/