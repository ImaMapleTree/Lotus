namespace Lotus.Server.Interfaces;

public interface IServerVersionHandler: IServerPatchHandler
{
    public bool GetBroadcastVersion(bool isLocal, out int serverVersion);

    object IServerPatchHandler.Execute(params object?[] parameters)
    {
        return GetBroadcastVersion((bool)parameters[0]!, out int _);
    }
}