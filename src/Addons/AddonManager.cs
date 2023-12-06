using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Lotus.RPC;
using Lotus.Extensions;
using Lotus.Roles2;
using VentLib;
using VentLib.Networking.RPC.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Addons;

public class AddonManager
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(AddonManager));

    public static LogLevel AddonLL = LogLevel.Info.Similar("ADDON", ConsoleColor.Magenta);
    public static List<LotusAddon> Addons = new();

    public static void ImportAddons()
    {
        DirectoryInfo addonDirectory = new("./addons/");
        if (!addonDirectory.Exists)
            addonDirectory.Create();
        addonDirectory.EnumerateFiles().Do(LoadAddon);
        Addons.ForEach(addon =>
        {
            log.Log(AddonLL, $"Calling Post Initialize for {addon.Name}");
            addon.PostInitialize(new List<LotusAddon>(Addons));
        });

        Addons.ForEach(addon =>
        {
            log.Info($"Found and registering {addon.ExportedDefinitions.Count} role definitions from {addon.Name}");
            addon.ExportedDefinitions.ForEach(kvp => kvp.Value.ForEach(gm => gm.RoleManager.RegisterRole(ProjectLotus.DefinitionUnifier.Unify(kvp.Key))));
        });
    }

    private static void LoadAddon(FileInfo file)
    {
        try
        {
            Assembly assembly = Assembly.LoadFile(file.FullName);
            Type? lotusType = assembly.GetTypes().FirstOrDefault(t => t.IsAssignableTo(typeof(LotusAddon)));
            if (lotusType == null)
                throw new ConstraintException($"Lotus Addons requires ONE class file that extends {nameof(LotusAddon)}");
            LotusAddon addon = (LotusAddon)AccessTools.Constructor(lotusType).Invoke(Array.Empty<object>());

            log.Log(AddonLL,$"Loading Addon [{addon.Name} {addon.Version}]", "AddonManager");
            Vents.Register(assembly);

            Addons.Add(addon);
            addon.Initialize();
            ProjectLotus.DefinitionUnifier.RegisterRoleComponents(assembly);
        }
        catch (Exception e)
        {
            log.Exception($"Exception encountered while loading addon: {file.Name}", e);
        }
    }

    [ModRPC((uint)ModCalls.VerifyAddons, RpcActors.NonHosts, RpcActors.Host)]
    public static void VerifyClientAddons(List<AddonInfo> addons)
    {
        List<AddonInfo> hostInfo = Addons.Select(AddonInfo.From).ToList();
        int[] senderId = { Vents.GetLastSender((uint)ModCalls.VerifyAddons)?.GetClientId() ?? 999 };
        $"Last Sender: {senderId}".DebugLog();

        List<AddonInfo> mismatchInfo = Addons.Select(hostAddon =>
        {
            AddonInfo haInfo = AddonInfo.From(hostAddon);
            AddonInfo matchingAddon = addons.FirstOrDefault(a => a == haInfo);
            if (matchingAddon == null)
            {
                haInfo.Mismatches = Mismatch.ClientMissingAddon;
                Vents.BlockClient(hostAddon.BundledAssembly, senderId[0]);
                return haInfo;
            }

            matchingAddon.CheckVersion(matchingAddon);
            return matchingAddon;
        }).ToList();

        mismatchInfo.AddRange(addons.Select(clientAddon =>
            {
                AddonInfo matchingAddon = hostInfo.FirstOrDefault(a => a == clientAddon);
                if (matchingAddon == null)
                    clientAddon.Mismatches = Mismatch.HostMissingAddon;
                else
                    clientAddon.CheckVersion(matchingAddon);
                return clientAddon;
            }));

        mismatchInfo.DistinctBy(addon => addon.Name).Where(addon => addon.Mismatches is not (Mismatch.None or Mismatch.ClientMissingAddon)).Do(a => Vents.FindRPC(1017)!.Send(senderId, a.AssemblyFullName, 0));
        ReceiveAddonVerification(mismatchInfo.DistinctBy(addon => addon.Name).Where(addon => addon.Mismatches is not Mismatch.None).ToList());
    }

    [ModRPC((uint)ModCalls.VerifyAddons, RpcActors.Host, RpcActors.LastSender)]
    public static void ReceiveAddonVerification(List<AddonInfo> addons)
    {
        if (addons.Count == 0) return;
        log.Exception(" Error Validating Addons. All CustomRPCs between the host and this client have been disabled.", "VerifyAddons");
        log.Exception(" -=-=-=-=-=-=-=-=-=[Errored Addons]=-=-=-=-=-=-=-=-=-", "VerifyAddons");
        foreach (var rejectReason in addons.Where(info => info.Mismatches is not Mismatch.None).Select(addonInfo => (addonInfo.Mismatches)
             switch {
                 Mismatch.Version => $" {addonInfo.Name}:{addonInfo.Version} => Local version is not compatible with the host version of the addon",
                 Mismatch.ClientMissingAddon => $" {addonInfo.Name}:{addonInfo.Version} => Client Missing Addon ",
                 Mismatch.HostMissingAddon => $" {addonInfo.Name}:{addonInfo.Version} => Host Missing Addon ",
                 _ => throw new ArgumentOutOfRangeException()
             }))
            log.Exception(rejectReason, "VerifyAddons");
    }
}