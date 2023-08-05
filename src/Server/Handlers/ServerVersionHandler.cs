using Lotus.Server.Interfaces;

namespace Lotus.Server.Handlers;

internal class ServerVersionHandlers
{
    public static IServerVersionHandler StandardHandler = new Standard();
    public static IServerVersionHandler ProtectionPatchedHandler = new ProtectionPatched();

    private class Standard: IServerVersionHandler
    {
        public bool GetBroadcastVersion(bool isLocal, out int serverVersion)
        {
            serverVersion = 0;
            if (isLocal) return false;
            serverVersion = Constants.GetVersion(2222, 0, 0, 0);
            return true;
        }
    }

    private class ProtectionPatched: IServerVersionHandler
    {
        public bool GetBroadcastVersion(bool isLocal, out int serverVersion)
        {
            serverVersion = 0;
            return false;
        }
    }
}