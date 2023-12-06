using System.Collections.Generic;
using Lotus.Server.Handlers;
using Lotus.Server.Interfaces;

namespace Lotus.Server.Patches;

public class UnpatchedServerImplementation: AbstractServerPatch
{
    protected override Dictionary<PatchedCode, IServerPatchHandler> CodePatchHandlers { get; set; } = new()
    {
        { PatchedCode.ServerVersion, ServerVersionHandlers.StandardHandler },
    };
}