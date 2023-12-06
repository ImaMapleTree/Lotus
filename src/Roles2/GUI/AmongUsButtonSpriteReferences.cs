using UnityEngine;

namespace Lotus.Roles2.GUI;

public class AmongUsButtonSpriteReferences
{
    public static ReportButton ReportButton => HudManager.Instance != null ? HudManager.Instance.ReportButton : null!;
    public static Sprite ReportButtonSprite => ReportButton != null ? ReportButton.graphic.sprite : null!;

    public static VentButton VentButton => HudManager.Instance != null ? HudManager.Instance.ImpostorVentButton : null!;
    public static Sprite VentButtonSprite => VentButton != null ? VentButton.graphic.sprite : null!;

    public static UseButton UseButton => HudManager.Instance != null ? HudManager.Instance.UseButton : null!;
    public static Sprite UseButtonSprite => UseButton != null ? UseButton.graphic.sprite : null!;

    public static PetButton PetButton => HudManager.Instance != null ? HudManager.Instance.PetButton : null!;
    public static Sprite PetButtonSprite => PetButton != null ? PetButton.graphic.sprite : null!;

    public static KillButton KillButton => HudManager.Instance != null ? HudManager.Instance.KillButton : null!;
    public static Sprite KillButtonSprite => KillButton != null ? KillButton.graphic.sprite : null!;

    public static AbilityButton AbilityButton => HudManager.Instance != null ? HudManager.Instance.AbilityButton : null!;
    public static Sprite AbilityButtonSprite => AbilityButton != null ? AbilityButton.graphic.sprite : null!;
}