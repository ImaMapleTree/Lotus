using System;
using System.Collections.Generic;
using TOHTOR.API.Reactive;
using TOHTOR.API.Reactive.HookEvents;
using TOHTOR.Extensions;
using TOHTOR.Utilities;
using VentLib.Networking.RPC;
using VentLib.Utilities.Attributes;

namespace TOHTOR.Managers.Reporting;

[LoadStatic]
class RpcReporter: IReportProducer
{
    private const string ReporterHookKey = nameof(RpcReporter);
    private static RpcReporter _reporter = new();
    private List<(DateTime, RpcMeta)> rpcs = new();

    private RpcReporter()
    {
        Hooks.GameStateHooks.GameStartHook.Bind(ReporterHookKey, RefreshOnState);
        Hooks.GameStateHooks.GameEndHook.Bind(ReporterHookKey, RefreshOnState);
        Hooks.NetworkHooks.RpcHook.Bind(ReporterHookKey, HandleRpcEvent);
        ReportManager.AddProducer(this, ReportTag.KickByAnticheat);
    }

    private void RefreshOnState(GameStateHookEvent hookEvent)
    {
        rpcs.Clear();
    }

    private void HandleRpcEvent(RpcHookEvent rpcHookEvent)
    {
        rpcs.Add((DateTime.Now, rpcHookEvent.Meta));
    }

    public ReportInfo ProduceReport() => ReportInfo.Create("Rpc History", "rpc-history").Attach(CreateRpcContent());

    private string CreateRpcContent()
    {
        string content = "";
        rpcs.ForEach(tuple =>
        {
            string timestamp = tuple.Item1.ToString("hh:mm:ss");
            RpcMeta meta = tuple.Item2;
            string target = Utils.PlayerById((int)meta.NetId).Map(p => p.UnalteredName()).OrElse("Unknown") + $" (Id: {meta.NetId})";
            string recipient = Utils.PlayerByClientId(meta.Recipient).Map(p => p.UnalteredName()).OrElse("Unknown") + $" (Id: {meta.Recipient})";
            string rpc = ((RpcCalls)meta.CallId).Name();

            content += $"[{timestamp}] (Target: {target}, Recipient: {recipient}, RPC: {rpc}, Immediate: {meta.Immediate}, SendOptions: {meta.SendOption}, RequiresHost: {meta.RequiresHost})\n";
        });
        return content;
    }
}