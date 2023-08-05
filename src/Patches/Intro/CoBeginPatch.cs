using System;
using System.Linq;
using HarmonyLib;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Utilities;
using VentLib.Utilities;
using VentLib.Version;
using Version = VentLib.Version.Version;

namespace Lotus.Patches.Intro;

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
class CoBeginPatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(CoBeginPatch));

    public static void Prefix()
    {
        Game.State = GameState.InIntro;
        log.Info("------------名前表示------------", "Info");
        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            log.Info($"{(pc.AmOwner ? "[*]" : ""),-3}{pc.PlayerId,-2}:{pc.name.PadRightV2(20)}:{pc.cosmetics.nameText.text}({Palette.ColorNames[pc.Data.DefaultOutfit.ColorId].ToString().Replace("Color", "")})", "Info");
            pc.cosmetics.nameText.text = pc.name;
        }
        log.Info("----------役職割り当て----------", "Info");
        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            log.Info($"{(pc.AmOwner ? "[*]" : ""),-3}{pc.PlayerId,-2}:{pc.name.PadRightV2(20)}:{pc.GetAllRoleName()}", "Info");
        }
        log.Info("--------------環境--------------", "Info");
        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            try
            {
                var text = pc.AmOwner ? "[*]" : "   ";
                text += $"{pc.PlayerId,-2}:{pc.name.PadRightV2(20)}:{pc.GetClient()?.PlatformData?.Platform.ToString()?.Replace("Standalone", ""),-11}";

                Version version = ModVersion.VersionControl.GetPlayerVersion(pc.PlayerId);
                text += version == new NoVersion() ? ":Vanilla" : $":Mod({version})";
                log.Info(text, "Info");
            }
            catch (Exception ex)
            {
                log.Exception("Platform", ex);
            }
        }
        log.Info("------------基本設定------------", "Info");
        var tmp = GameOptionsManager.Instance.CurrentGameOptions.ToHudString(GameData.Instance ? GameData.Instance.PlayerCount : 10).Split("\r\n").Skip(1);
        foreach (var t in tmp) log.Info(t, "Info");
        log.Info("------------詳細設定------------", "Info");
        log.Info($"プレイヤー数: {PlayerControl.AllPlayerControls.Count}人", "Info");
        //PlayerControl.AllPlayerControls.ToArray().Do(x => TOHPlugin.PlayerStates[x.PlayerId].InitTask(x));
    }
}