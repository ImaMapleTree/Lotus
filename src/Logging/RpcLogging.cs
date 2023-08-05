using VentLib.Networking.RPC;
using VentLib.Utilities.Attributes;

namespace Lotus.Logging;

[LoadStatic]
public class RpcLogging
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(RpcLogging));
    private static readonly LogLevel RpcLevel = new("RPC", color: System.Drawing.Color.Coral);

    static RpcLogging()
    {
        RpcMeta.AddSubscriber(LogRpc);
    }

    public static void LogRpc(RpcMeta rpcMeta)
    {
        log.Log(RpcLevel, rpcMeta.ToString());
    }
}