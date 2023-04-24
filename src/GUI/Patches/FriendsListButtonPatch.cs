using HarmonyLib;
using UnityEngine;

namespace TOHTOR.GUI.Patches;

[HarmonyPatch(typeof(FriendsListButton), nameof(FriendsListButton.OnSceneLoaded))]
public class FriendsListButtonPatch
{
    public static void Postfix(FriendsListButton __instance)
    {
        __instance.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
        Object.FindObjectOfType<FriendsListButton>().transform.position = new Vector3(-2.15f, 2.22f, 0f);
    }

    public static void FixFriendListPosition()
    {
        FriendsListButton friendsListButton = Object.FindObjectOfType<FriendsListButton>();
        var transform = friendsListButton.transform;
        transform.localScale = new Vector3(1f, 1f, 1f);
        transform.localPosition = new Vector3(0f, 0f, 0f);
    }
}