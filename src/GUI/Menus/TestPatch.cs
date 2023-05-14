using HarmonyLib;

namespace Lotus.GUI.Menus;

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public class TestPatch
{
    public static void Postfix(LobbyBehaviour __instance)
    {
        HistoryMenuIntermediate.Initialize();
    }
}