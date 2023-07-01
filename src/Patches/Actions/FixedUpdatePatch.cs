using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.Roles.Internals;
using Lotus.Extensions;
using Lotus.Roles.Internals.Enums;
using LotusTrigger.Options;
using VentLib.Utilities;
using VentLib.Utilities.Debug.Profiling;
using VentLib.Utilities.Extensions;

namespace Lotus.Patches.Actions;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
static class FixedUpdatePatch
{
    private static void Postfix(PlayerControl __instance)
    {
        Game.RecursiveCallCheck = 0;
        DisplayModVersion(__instance);

        if (!AmongUsClient.Instance.AmHost) return;

        if (!__instance.IsHost() && Game.State is GameState.InLobby && __instance.Data.PlayerLevel < GeneralOptions.AdminOptions.KickPlayersUnderLevel)
            AmongUsClient.Instance.KickPlayer(__instance.GetClientId(), false);

        if (Game.State is not GameState.Roaming) return;
        uint id = Profilers.Global.Sampler.Start("Fixed Update Patch");

        var player = __instance;
        ActionHandle handle = null;
        __instance.Trigger(LotusActionType.FixedUpdate, ref handle);

        if (player.IsAlive() && GeneralOptions.GameplayOptions.EnableLadderDeath) FallFromLadder.FixedUpdate(player);
        Profilers.Global.Sampler.Stop(id);
        /*if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) DisableDevice.FixedUpdate();*/
        /*EnterVentPatch.CheckVentSwap(__instance);*/
    }

    private static void DisplayModVersion(PlayerControl player)
    {
        if (Game.State is not GameState.InLobby) return;
        /*if (!TOHPlugin.playerVersion.TryGetValue(player.PlayerId, out var ver)) return;
        /*if (TOHPlugin.ForkId != ver.forkId) // フォークIDが違う場合
            player.cosmetics.nameText.text = $"<color=#ff0000><size=1.2>{ver.forkId}</size>\n{player?.name}</color>";#1#
        if (TOHPlugin.version.CompareTo(ver.version) == 0)
            player.cosmetics.nameText.text = ver.tag == $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})" ? $"<color=#87cefa>{player.name}</color>" : $"<color=#ffff00><size=1.2>{ver.tag}</size>\n{player?.name}</color>";
        else player.cosmetics.nameText.text = $"<color=#ff0000><size=1.2>v{ver.version}</size>\n{player?.name}</color>";*/
    }
}