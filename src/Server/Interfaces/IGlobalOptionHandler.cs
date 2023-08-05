using Lotus.Roles.Overrides;
using VentLib.Utilities.Collections;

namespace Lotus.Server.Interfaces;

internal interface IGlobalOptionHandler: IServerPatchHandler
{
    public RemoteList<GameOptionOverride> GetGlobalOptions();

    object IServerPatchHandler.Execute(params object?[] parameters) => GetGlobalOptions();
}