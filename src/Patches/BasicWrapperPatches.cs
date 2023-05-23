using Lotus.API.Odyssey;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.Patches;

public class BasicWrapperPatches
{
    //private static readonly Dictionary<byte, string> PlayerNames = new();

    [QuickPrefix(typeof(PlayerControl), nameof(PlayerControl.RawSetName))]
    public static bool WrapSetNamePatch(PlayerControl __instance, string name)
    {
        __instance.cosmetics.SetName(name);
        __instance.cosmetics.SetNameMask(true);
        if (Game.State is GameState.InLobby) __instance.name = name;
        return false;
    }

    /*[QuickPrefix(typeof(PlayerControl), nameof(PlayerControl.RpcSetName))]
    public static void WrapRpcSetNamePatch(PlayerControl __instance, ref string name)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (Game.State is not GameState.InLobby) return;
        if (__instance == null) return;
        
        CustomTitle? title = PluginDataManager.TitleManager.GetTitle(__instance.FriendCode);
        if (title == null) return;

        string playerName = name;
        string fullTitle = title.ApplyTo(name);

        Async.Schedule(() => FixChatName(__instance, title.ApplyTo(playerName, true)), 0.1f);
        PlayerNames[__instance.PlayerId] = playerName;
        name = fullTitle;
    }

    [QuickPostfix(typeof(PlayerControl), nameof(PlayerControl.RpcSetName))]
    public static void WrapRpcSetNamePostfix(PlayerControl __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (Game.State is not GameState.InLobby) return;
        PlayerNames.GetOptional(__instance.PlayerId).IfPresent(i => __instance.name = i);
    }
    

    private static void FixChatName(PlayerControl player, string chatName)
    {
        GameData.Instance.UpdateName(player.PlayerId, chatName, true);
    }*/
}