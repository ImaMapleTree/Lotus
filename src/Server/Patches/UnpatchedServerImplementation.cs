using System.Collections.Generic;
using Lotus.Server.Handlers;
using Lotus.Server.Interfaces;

namespace Lotus.Server.Patches;

public class UnpatchedServerImplementation: AbstractServerPatch
{
    protected override Dictionary<PatchedCode, IServerPatchHandler> CodePatchHandlers { get; set; } = new()
    {
        { PatchedCode.CheckMurder, CheckMurderHandlers.StandardHandler },
        { PatchedCode.MurderPlayer, MurderPlayerHandlers.StandardHandler },
        { PatchedCode.PreGameSetup, PreGameSetupHandlers.StandardHandler },
        { PatchedCode.RemoveProtection, RemoveProtectHandlers.StandardHandler },
        { PatchedCode.RpcMark, RpcMarkHandlers.StandardHandler },
        { PatchedCode.PostMeeting, PostMeetingHandlers.StandardHandler },
        { PatchedCode.ServerVersion, ServerVersionHandlers.StandardHandler },
        { PatchedCode.GlobalOverrides, GlobalOptionHandlers.StandardHandler }
    };
}