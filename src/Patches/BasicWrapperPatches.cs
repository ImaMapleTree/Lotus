using Lotus.API.Odyssey;
using Lotus.Utilities;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.Patches;

public class BasicWrapperPatches
{
    [QuickPrefix(typeof(PlayerControl), nameof(PlayerControl.RawSetName))]
    public static bool WrapSetNamePatch(PlayerControl __instance, string name)
    {
        __instance.cosmetics.SetName(name);
        __instance.cosmetics.SetNameMask(true);
        if (Game.State is GameState.InLobby) __instance.name = name.RemoveHtmlTags();
        return false;
    }
}