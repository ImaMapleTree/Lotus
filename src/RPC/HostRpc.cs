using System.Linq;
using Lotus.Options;
using Lotus.Extensions;
using Lotus.Utilities;
using VentLib;
using VentLib.Utilities.Extensions;
using VentLib.Logging;
using VentLib.Networking.RPC.Attributes;
using VentLib.Options;
using VentLib.Utilities.Collections;

namespace Lotus.RPC;

public static class HostRpc
{
    [ModRPC((uint) ModCalls.SendOptionPreview, RpcActors.Host, RpcActors.NonHosts)]
    public static void RpcSendOptions(BatchList<Option> options)
    {
        VentLogger.Debug($"Received {options.Count} Options from Host");
        OptionShower.GetOptionShower().Update();
    }

    [ModRPC((uint) ModCalls.Debug, RpcActors.Host, RpcActors.NonHosts)]
    public static void RpcDebug(string message)
    {
        VentLogger.Info($"Message from {Vents.GetLastSender((uint)ModCalls.Debug).name} => {message}", "RpcDebug");
        GameData.Instance.AllPlayers.ToArray().Select(p => (p.GetNameWithRole(), p.IsDead, p.IsIncomplete)).StrJoin().DebugLog("All Players: ");
    }
}