using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Reactive;
using Lotus.Server.Modifiers;
using Lotus.Server.Patches;
using VentLib.Utilities.Extensions;

namespace Lotus.Server;

public class ServerPatchManager
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(ServerPatchManager));
    public static IServerPatch Patch => ProjectLotus.ServerPatchManager.Amalgamate;

    public IServerPatch Amalgamate { get; private set; } = null!;

    private readonly List<IServerPatch> serverPatches = new() { new UnpatchedServerImplementation() };
    private readonly List<IPatchModifier> patchModifiers = new() { new PatchRoleInitializerModifier() };

    private readonly List<IServerPatch> patchBuffer = new();

    public ServerPatchManager()
    {
        Hooks.ModHooks.LotusInitializedHook.Bind(nameof(ServerPatchManager), () =>
        {
            patchBuffer.ForEach(AddPatch);
            patchBuffer.Clear();
            CreateAmalgamPatch();
        });
        CreateAmalgamPatch();
    }

    public void CreateAmalgamPatch() => Amalgamate = new AmalgamServerPatch(serverPatches);

    public void AddPatch(IServerPatch patch)
    {
        if (!ProjectLotus.FinishedLoading)
        {
            patchBuffer.Add(patch);
            return;
        }

        log.Debug($"Enabling server patch {patch.GetType()} (Modifiers={patch.GetPatchHandlers().Select(p => p.GetType().Name).Fuse()})");
        serverPatches.Add(patchModifiers.OrderByDescending(p => p.Priority().Value).Aggregate(patch, (accumulate, modifier) => modifier.Modify(accumulate)));
        CreateAmalgamPatch();
        Amalgamate.GetPatchHandlers().ForEach(ph => ph.OnEnable(Amalgamate));
    }

    public void RemovePatch(IServerPatch patch) => RemovePatch(patch.GetType());

    public void RemovePatch(Type serverPatch)
    {
        serverPatches.RemoveAll(p => p.GetType() == serverPatch);
        Amalgamate.GetPatchHandlers().ForEach(p =>
        {
            log.Trace($"Disabling server patch {p.GetType()}");
            p.OnDisable(Amalgamate);
        });
        log.Debug($"Successfully Disabled {Amalgamate.GetPatchHandlers().Count()} patches");
        CreateAmalgamPatch();
    }

    public void AddPatchModifier(IPatchModifier patchModifier)
    {
        log.Debug($"Adding patch modifier {patchModifier}");
        patchModifiers.Add(patchModifier);
    }

    public void RemovePatchModifier(IPatchModifier patchModifier)
    {
        log.Debug($"Removing patch modifier {patchModifier}");
        patchModifiers.Remove(patchModifier);
    }
}