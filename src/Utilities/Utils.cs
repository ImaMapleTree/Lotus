using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Hazel;
using InnerNet;
using Lotus.API.Odyssey;
using Lotus.Chat.Patches;
using Lotus.GUI.Name.Holders;
using Lotus.Managers;
using Lotus.Options;
using Lotus.Roles.Extra;
using Lotus.Roles.Interfaces;
using Lotus.API;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Chat;
using Lotus.Extensions;
using Lotus.Roles;
using Lotus.Roles.Legacy;
using UnityEngine;
using VentLib.Localization;
using VentLib.Logging;
using VentLib.Networking.RPC;
using VentLib.Networking.RPC.Interfaces;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Utilities;

public static class Utils
{
    public static string GetNameWithRole(this GameData.PlayerInfo player)
    {
        return GetPlayerById(player.PlayerId)?.GetNameWithRole() ?? "";
    }

    public static Color? ConvertHexToColor(string hex)
    {
        if (!ColorUtility.TryParseHtmlString(hex, out Color c)) return null;
        return c;
    }

    public static bool HasTasks(GameData.PlayerInfo p)
    {
        if (p.GetCustomRole().RealRole.IsImpostor()) return false;
        return p.GetCustomRole() is ITaskHolderRole taskHolderRole && taskHolderRole.HasTasks();
    }

    /*public static void ShowActiveSettings(byte PlayerId = byte.MaxValue)
    {
        var mapId = TOHPlugin.NormalOptions.MapId;
        var text = "";
        if (OldOptions.CurrentGameMode == CustomGameMode.HideAndSeek)
        {
            text = GetString("Roles") + ":";
            if (Fox.Ref<Fox>().IsEnable())
                text += String.Format("\n{0}:{1}", GetRoleName(Fox.Ref<Fox>()), Fox.Ref<Fox>().Count);
            if (Troll.Ref<Troll>().IsEnable())
                text += String.Format("\n{0}:{1}", GetRoleName(Troll.Ref<Troll>()), Troll.Ref<Troll>().Count);
            SendMessage(text, PlayerId);
            text = GetString("Settings") + ":";
            text += GetString("HideAndSeek");
        }
        else
        {
            text = GetString("Settings") + ":";
            foreach (var role in OldOptions.CustomRoleCounts)
            {
                if (!role.Key.GetReduxRole().IsEnable()) continue;
                text += $"\n【{GetRoleName(role.Key.GetReduxRole())}×{role.Key.GetReduxRole().Count}】\n";
                ShowChildrenSettings(OldOptions.CustomRoleSpawnChances[role.Key], ref text);
                text = text.RemoveHtmlTags();
            }

            foreach (var opt in OptionItem.AllOptions.Where(x =>
                         x.GetBool() && x.Parent == null && x.Id >= 80000 &&
                         !x.IsHiddenOn(OldOptions.CurrentGameMode)))
            {
                if (opt.Name is "KillFlashDuration" or "RoleAssigningAlgorithm")
                    text += $"\n【{opt.GetName(true)}: {{opt.GetString()}}】\n";
                else
                    text += $"\n【{opt.GetName(true)}】\n";
                ShowChildrenSettings(opt, ref text);
                text = text.RemoveHtmlTags();
            }
        }

        SendMessage(text, PlayerId);
    }*/

    /*public static void ShowActiveRoles(byte PlayerId = byte.MaxValue)
    {
        var text = GetString("Roles") + ":";
        text += string.Format("\n{0}:{1}", GetRoleName(GM.Ref<GM>()),
            StaticOptions.EnableGM.ToString().RemoveHtmlTags());
        foreach (CustomRoles role in Enum.GetValues(typeof(CustomRoles)))
        {
            if (role is CustomRoles.HASFox or CustomRoles.HASTroll) continue;
            if (role.GetReduxRole().IsEnable())
                text += string.Format("\n{0}:{1}x{2}", GetRoleName(role.GetReduxRole()),
                    $"{role.GetReduxRole().Chance * 100}%", role.GetReduxRole().Count);
        }

        SendMessage(text, PlayerId);
    }*/

    public static void Teleport(CustomNetworkTransform nt, Vector2 location)
    {
        Vector2 currentLocation = nt.prevPosSent;
        Hooks.PlayerHooks.PlayerTeleportedHook.Propagate(new PlayerTeleportedHookEvent(nt.myPlayer, currentLocation, location));
        TeleportDeferred(nt, location).Send();
    }

    public static MonoRpc TeleportDeferred(PlayerControl player, Vector2 location) => TeleportDeferred(player.NetTransform, location);

    public static MonoRpc TeleportDeferred(CustomNetworkTransform transform, Vector2 location)
    {
        if (AmongUsClient.Instance.AmHost) transform.SnapTo(location);
        return RpcV3.Immediate(transform.NetId, RpcCalls.SnapTo, SendOption.None).Write(location).Write(transform.lastSequenceId);
    }

    /*public static void ShowLastResult(byte PlayerId = byte.MaxValue)
    {
        if (AmongUsClient.Instance.IsGameStarted)
        {
            SendMessage(GetString("CantUse.lastroles"), PlayerId);
            return;
        }

        var text = GetString("LastResult") + ":";
        List<byte> cloneRoles = new(TOHPlugin.PlayerStates.Keys);
        text += $"\n{SetEverythingUpPatch.LastWinsText}\n";
        /*foreach (var id in TOHPlugin.winnerList)
        {
            text += $"\n★ " + EndGamePatch.SummaryText[id].RemoveHtmlTags();
            cloneRoles.Remove(id);
        }#1#

        foreach (var id in cloneRoles)
        {
            text += $"\n　 " + EndGamePatch.SummaryText[id].RemoveHtmlTags();
        }

        SendMessage(text, PlayerId);
        SendMessage(EndGamePatch.KillLog, PlayerId);
    }
    */


    public static string GetSubRolesText(byte id, bool disableColor = false)
    {
        PlayerControl player = GetPlayerById(id)!;
        return player.NameModel().GetComponentHolder<SubroleHolder>().Render(player, GameState.Roaming);
    }

    /*public static void ShowHelp()
    {
        SendMessage(
            GetString("CommandList")
            + $"\n/winner - {GetString("Command.winner")}"
            + $"\n/lastresult - {GetString("Command.lastresult")}"
            + $"\n/rename - {GetString("Command.rename")}"
            + $"\n/now - {GetString("Command.now")}"
            + $"\n/h now - {GetString("Command.h_now")}"
            + $"\n/h roles {GetString("Command.h_roles")}"
            + $"\n/h addons {GetString("Command.h_addons")}"
            + $"\n/h modes {GetString("Command.h_modes")}"
            + $"\n/dump - {GetString("Command.dump")}"
        );

    }*/

    public static PlayerControl? GetPlayerById(int playerId) => PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(pc => pc.PlayerId == playerId);

    public static Optional<PlayerControl> PlayerById(int playerId) => PlayerControl.AllPlayerControls.ToArray().FirstOrOptional(pc => pc.PlayerId == playerId);

    public static Optional<PlayerControl> PlayerByClientId(int clientId)
    {
        return PlayerControl.AllPlayerControls.ToArray().FirstOrOptional(c => c.GetClientId() == clientId);
    }

    public static string GetVoteName(byte num)
    {
        string name = "invalid";
        var player = GetPlayerById(num);
        if (num < 15 && player != null) name = player?.GetNameWithRole();
        if (num == 253) name = "Skip";
        if (num == 254) name = "None";
        if (num == 255) name = "Dead";
        return name;
    }

    public static string PadRightV2(this object text, int num)
    {
        int bc = 0;
        var t = text.ToString();
        foreach (char c in t) bc += Encoding.GetEncoding("UTF-8").GetByteCount(c.ToString()) == 1 ? 1 : 2;
        return t?.PadRight(Mathf.Max(num - (bc - t.Length), 0));
    }


    public static string RemoveHtmlTags(this string str) => Regex.Replace(str, "<[^>]*?>", "");

    public static void FlashColor(Color color, float duration = 1f)
    {
        var hud = DestroyableSingleton<HudManager>.Instance;
        if (hud.FullScreen == null) return;
        var obj = hud.transform.FindChild("FlashColor_FullScreen")?.gameObject;
        if (obj == null)
        {
            obj = GameObject.Instantiate(hud.FullScreen.gameObject, hud.transform);
            obj.name = "FlashColor_FullScreen";
        }

        hud.StartCoroutine(Effects.Lerp(duration, new Action<float>((t) =>
        {
            obj.SetActive(t != 1f);
            obj.GetComponent<SpriteRenderer>().color = new(color.r, color.g, color.b,
                Mathf.Clamp01((-2f * Mathf.Abs(t - 0.5f) + 1) * color.a)); //アルファ値を0→目標→0に変化させる
        })));
    }

    public static Sprite LoadSprite(string path, float pixelsPerUnit = 100f, bool linear = false, int mipMapLevel = 0)
    {
        Sprite sprite = null;
        try
        {
            var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(path);
            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, true, linear);
            using MemoryStream ms = new();
            stream.CopyTo(ms);
            ImageConversion.LoadImage(texture, ms.ToArray());
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.texture.requestedMipmapLevel = mipMapLevel;
        }
        catch (Exception e)
        {
            VentLogger.Error($"Error Loading Asset: \"{path}\"", "LoadImage");
            VentLogger.Exception(e, "LoadImage");
        }

        return sprite;
    }

    public static AudioClip LoadAudioClip(string path, string clipName = "UNNAMED_TOR_AUDIO_CLIP")
    {
        // must be "raw (headerless) 2-channel signed 32 bit pcm (le)" (can e.g. use Audacity� to export)
        try
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(path);
            var byteAudio = new byte[stream.Length];
            _ = stream.Read(byteAudio, 0, (int)stream.Length);
            float[] samples = new float[byteAudio.Length / 4]; // 4 bytes per sample
            int offset;
            for (int i = 0; i < samples.Length; i++)
            {
                offset = i * 4;
                samples[i] = (float)BitConverter.ToInt32(byteAudio, offset) / Int32.MaxValue;
            }

            int channels = 2;
            int sampleRate = 48000;
            AudioClip audioClip = AudioClip.Create(clipName, samples.Length, channels, sampleRate, false);
            audioClip.SetData(samples, 0);
            return audioClip;
        }
        catch
        {
            System.Console.WriteLine("Error loading AudioClip from resources: " + path);
        }

        return null;

        /* Usage example:
        AudioClip exampleClip = Helpers.loadAudioClipFromResources("Lotus.assets.exampleClip.raw");
        if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(exampleClip, false, 0.8f);
        */
    }

    public static string ColorString(Color32 color, string str) =>
        $"<color=#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}>{str}</color>";

    public static string GetOnOffColored(bool value) =>
        value ? Color.cyan.Colorize("ON") : Color.red.Colorize("OFF");

    public static void RunUntilSuccess(Action action, float waitTime, Func<bool>? predicate = null)
    {
        Action moddedAction = action;
        if (predicate == null)
        {
            bool localPredicate = false;
            moddedAction = () =>
            {
                try {
                    action();
                    localPredicate = true;
                }
                catch { }
            };
            predicate = () => localPredicate;
        }

        void SuccessMethod()
        {
            Async.Schedule(() =>
            {
                moddedAction();
                if (!predicate()) SuccessMethod();
            }, waitTime);
        }

        SuccessMethod();
    }
}