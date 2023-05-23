using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP;
using Lotus;
using Lotus.Addons;
using Lotus.API;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Chat;
using Lotus.Gamemodes;
using Lotus.GUI.Menus;
using Lotus.GUI.Patches;
using Lotus.Managers;
using Lotus.Managers.Reporting;
using Lotus.Options;
using Lotus.Roles.Internals.Attributes;
using Lotus.RPC;
using Lotus.Utilities;
using VentLib;
using VentLib.Logging;
using VentLib.Networking.Handshake;
using VentLib.Networking.RPC;
using VentLib.Options;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Version;
using VentLib.Version.Git;
using VentLib.Version.Updater;
using VentLib.Version.Updater.Github;
using Version = VentLib.Version.Version;

[assembly: AssemblyVersion(ProjectLotus.CompileVersion)]
namespace Lotus;

[BepInPlugin(PluginGuid, "Lotus", $"{MajorVersion}.{MinorVersion}.0")]
[BepInProcess("Among Us.exe")]
public class ProjectLotus : BasePlugin, IGitVersionEmitter
{
    public const string PluginGuid = "com.tealeaf.Lotus";
    public const string CompileVersion = $"{MajorVersion}.{MinorVersion}.*";

    public const string MajorVersion = "1";
    public const string MinorVersion = "0"; // Update with each release

    public static string PluginVersion = typeof(ProjectLotus).Assembly.GetName().Version!.ToString();

    public readonly GitVersion CurrentVersion = new();

    public static readonly string ModName = "Project Lotus";
    public static readonly string ModColor = "#4FF918";

    public static readonly bool ShowDiscordButton = true;
    public static readonly string DiscordInviteUrl = "https://discord.gg/tohtor";

    public static readonly bool DevVersion = true;
    public static readonly string DevVersionStr = "Alpha 12.05.2023";

    public Harmony Harmony { get; } = new(PluginGuid);
    public static string CredentialsText;

    public static RProfiler Profiler = new("General");
    public static bool Initialized;

    public static ModUpdater ModUpdater = null!;
    


    public ProjectLotus()
    {
        Instance = this;
        Vents.Initialize();
        VersionControl versionControl = ModVersion.VersionControl = VersionControl.For(this);
        versionControl.AddVersionReceiver(ReceiveVersion);
        PluginDataManager.TemplateManager.RegisterTag("lobby-join", "Tag for the template shown to players joining the lobby.");
        
        ModUpdater = ModUpdater.Default();
        ModUpdater.EstablishConnection("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");
        ModUpdater.RegisterReleaseCallback(BeginUpdate, true);
    }

    private void BeginUpdate(Release release)
    {
        if (Constraints.DLLConstraint.Enabled) return;
        SplashPatch.UpdateButton.IfPresent(b => b.gameObject.SetActive(true));
        ModUpdateMenu.AddUpdateItem("Lotus", null, ex => ModUpdater.Update(errorCallback: ex)!);
        Assembly ventAssembly = typeof(Vents).Assembly;

        if (release.ContainsDLL($"{ventAssembly.GetName().Name!}.dll"))
            ModUpdateMenu.AddUpdateItem("VentFramework", null, ex => ModUpdater.Update(ventAssembly, ex)!);
    }


    public static NormalGameOptionsV07 NormalOptions => GameOptionsManager.Instance.currentNormalGameOptions;


    public static float RefixCooldownDelay = 0f;
    public static List<byte> ResetCamPlayerList = null!;

    public static GamemodeManager GamemodeManager;
    public static ProjectLotus Instance = null!;

    public override void Load()
    {
        //Profilers.Global.SetActive(false);
        ReportManager.AddProducer(Profiler);
        uint id = Profiler.Sampler.Start();
        GameOptionController.Enable();
        GamemodeManager = new GamemodeManager();

        BanManager.Init();

        VentLogger.Info($"{Application.version}", "AmongUs Version");
        VentLogger.Info(CurrentVersion.ToString(), "GitVersion");

        // Setup, order matters here

        int _ = CustomRoleManager.AllRoles.Count;
        StaticEditor.Register(Assembly.GetExecutingAssembly());
        Harmony.PatchAll(Assembly.GetExecutingAssembly());
        AddonManager.ImportAddons();

        GamemodeManager.Setup();
        ShowerPages.InitPages();
        //OptionManager.AllHolders.AddRange(OptionManager.Options().SelectMany(opt => opt.GetHoldersRecursive()));
        Initialized = true;
        Profiler.Sampler.Stop(id);

    }

    public GitVersion Version() => CurrentVersion;

    public HandshakeResult HandshakeFilter(Version handshake)
    {
        if (handshake is NoVersion) return HandshakeResult.FailDoNothing;
        if (handshake is AmongUsMenuVersion) return HandshakeResult.Ban;
        if (handshake is not GitVersion git) return HandshakeResult.DisableRPC;
        if (git.MajorVersion != CurrentVersion.MajorVersion && git.MinorVersion != CurrentVersion.MinorVersion) return HandshakeResult.FailDoNothing;
        return HandshakeResult.PassDoNothing;
    }

    private static void ReceiveVersion(Version version, PlayerControl player)
    {
        if (version is not NoVersion)
        {
            ModRPC rpc = Vents.FindRPC((uint)ModCalls.SendOptionPreview)!;
            rpc.Send(new[] { player.GetClientId() }, new BatchList<Option>(OptionManager.GetManager().GetOptions()));
        }

        if (PluginDataManager.TemplateManager.TryFormat(player, "lobby-join", out string message)) ChatHandler.Of(message).Send(player);
        Hooks.NetworkHooks.ReceiveVersionHook.Propagate(new ReceiveVersionHookEvent(player, version));
    }
}