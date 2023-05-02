using HarmonyLib;
using TOHTOR.Extensions;
using TOHTOR.Roles;
using UnityEngine;
using VentLib.Utilities.Harmony.Attributes;

namespace TOHTOR.GUI.Hud;

public class HighlightPatches
{
    private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
    private static readonly int AddColor = Shader.PropertyToID("_AddColor");

    [QuickPostfix(typeof(PlayerControl), nameof(PlayerControl.ToggleHighlight))]
    public static void TogglePlayerHighlight(PlayerControl __instance)
    {
        var player = PlayerControl.LocalPlayer;
        if (player.Data.IsDead) return;
        __instance.cosmetics.currentBodySprite.BodySprite.material.SetColor(OutlineColor, player.GetCustomRole().RoleColor);
    }

    [QuickPostfix(typeof(Vent), nameof(Vent.SetOutline))]
    public static void SetVentOutline(Vent __instance, [HarmonyArgument(1)] ref bool mainTarget)
    {
        CustomRole role = PlayerControl.LocalPlayer.GetCustomRole();

        __instance.myRend.material.SetColor(OutlineColor, role.RoleColor);
        __instance.myRend.material.SetColor(AddColor, mainTarget ? role.RoleColor : Color.clear);
    }
}