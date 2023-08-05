using Lotus.Server.Interfaces;

namespace Lotus.Server;

public interface IModifyableServerPatch: IServerPatch
{
    public void SetHandler(PatchedCode patchedCode, IServerPatchHandler serverPatchHandler);
}