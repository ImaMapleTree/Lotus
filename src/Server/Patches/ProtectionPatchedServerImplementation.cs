using System.Collections.Generic;
using Lotus.Server.Handlers;
using Lotus.Server.Interfaces;

namespace Lotus.Server.Patches;

public class ProtectionPatchedServerImplementation: AbstractServerPatch
{
    public static ProtectionPatchedServerImplementation Instance = new();

    protected override Dictionary<PatchedCode, IServerPatchHandler> CodePatchHandlers { get; set; } = new()
    {
        { PatchedCode.CheckMurder, CheckMurderHandlers.ProtectionPatchedHandler },
        { PatchedCode.MurderPlayer, MurderPlayerHandlers.StandardHandler },
        { PatchedCode.PreGameSetup, PreGameSetupHandlers.ProtectionPatchedHandler },
        { PatchedCode.RemoveProtection, RemoveProtectHandlers.ProtectionPatchedHandler},
        { PatchedCode.RpcMark, RpcMarkHandlers.ProtectionPatchedHandler },
        { PatchedCode.PostMeeting, PostMeetingHandlers.ProtectionPatchedHandler },
        { PatchedCode.ServerVersion , ServerVersionHandlers.ProtectionPatchedHandler },
        { PatchedCode.GlobalOverrides , GlobalOptionHandlers.ProtectionPatchedHandler }
    };
}