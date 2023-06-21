using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Hazel;
using Lotus.API.Odyssey;
using Lotus.GUI.Name.Holders;
using Lotus.Roles.Interfaces;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Extensions;
using UnityEngine;
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

    public static void Teleport(CustomNetworkTransform nt, Vector2 location)
    {
        Vector2 currentLocation = nt.prevPosSent;
        Hooks.PlayerHooks.PlayerTeleportedHook.Propagate(new PlayerTeleportedHookEvent(nt.myPlayer, currentLocation, location));
        TeleportDeferred(nt, location).Send();
    }

    public static MonoRpc TeleportDeferred(CustomNetworkTransform transform, Vector2 location)
    {
        if (AmongUsClient.Instance.AmHost) transform.SnapTo(location);
        return RpcV3.Immediate(transform.NetId, RpcCalls.SnapTo, SendOption.None).Write(location).Write(transform.lastSequenceId);
    }

    public static string GetSubRolesText(byte id, bool disableColor = false)
    {
        PlayerControl player = GetPlayerById(id)!;
        return player.NameModel().GetComponentHolder<SubroleHolder>().Render(player, GameState.Roaming);
    }

    public static PlayerControl? GetPlayerById(int playerId) => PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(pc => pc.PlayerId == playerId);

    public static Optional<PlayerControl> PlayerById(int playerId) => PlayerControl.AllPlayerControls.ToArray().FirstOrOptional(pc => pc.PlayerId == playerId);

    public static Optional<PlayerControl> PlayerByClientId(int clientId)
    {
        return PlayerControl.AllPlayerControls.ToArray().FirstOrOptional(c => c.GetClientId() == clientId);
    }

    public static string PadRightV2(this object text, int num)
    {
        int bc = 0;
        var t = text.ToString();
        foreach (char c in t) bc += Encoding.GetEncoding("UTF-8").GetByteCount(c.ToString()) == 1 ? 1 : 2;
        return t?.PadRight(Mathf.Max(num - (bc - t.Length), 0));
    }


    public static string RemoveHtmlTags(this string str) => Regex.Replace(str, "<[^>]*?>", "");

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
}