using System.Collections.Generic;
using Lotus.Server.Handlers;
using Lotus.Server.Interfaces;

namespace Lotus.Server.Patches;

public class ProtectionPatchedServerImplementation: AbstractServerPatch
{
    public static ProtectionPatchedServerImplementation Instance = new();

    protected override Dictionary<PatchedCode, IServerPatchHandler> CodePatchHandlers { get; set; } = new()
    {
        { PatchedCode.ServerVersion , ServerVersionHandlers.ProtectionPatchedHandler },
        { PatchedCode.GlobalOverrides , GlobalOptionHandlers.ProtectionPatchedHandler }
    };
}