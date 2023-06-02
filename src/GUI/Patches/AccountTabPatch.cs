using HarmonyLib;
using UnityEngine;
using VentLib.Utilities;

namespace Lotus.GUI.Patches;

/// <summary>
/// This patch re-adjusts the account tab when opened to account for the changes made to the main menu UI
/// </summary>
[HarmonyPatch(typeof(AccountTab), nameof(AccountTab.Toggle))]
public class AccountTabPatch
{
    private static bool IsOpen(AccountTab tab) => tab.GetComponent<SlideOpen>().isOpen;

    public static void Postfix(AccountTab __instance)
    {
        if (IsOpen(__instance)) Async.Schedule(() => ModifyAccountTabLocation(__instance), 0.26f);
        else FixAccountTabLocation(__instance);
    }

    public static void FixAccountTabLocation(AccountTab accountTab)
    {
        var transform = accountTab.transform;
        transform.localScale = new Vector3(1f, 1f, 1f);
        transform.position = new Vector3(0f, 0f);
    }

    public static void ModifyAccountTabLocation(AccountTab accountTab)
    {
        var transform = accountTab.transform;
        transform.localScale = new Vector3(0.55f, 0.55f, 1f);
        transform.position = new Vector3(-4.25f, 1.5f, 0f);
    }
}