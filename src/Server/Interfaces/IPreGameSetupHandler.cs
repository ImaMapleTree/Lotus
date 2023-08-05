using System.Collections;

namespace Lotus.Server.Interfaces;

public interface IPreGameSetupHandler: IServerPatchHandler
{
    public IEnumerator PreGameSetup(PlayerControl player, string pet);

    object IServerPatchHandler.Execute(params object[] parameters)
    {
        return PreGameSetup((PlayerControl)parameters[0], (string)parameters[1]);
    }
}