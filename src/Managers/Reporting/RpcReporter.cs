using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Player;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Utilities;
using Lotus.Extensions;
using VentLib.Networking.RPC;
using VentLib.Utilities.Attributes;
using EnumerableExtensions = VentLib.Utilities.Extensions.EnumerableExtensions;

namespace Lotus.Managers.Reporting;

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
        ReportManager.AddProducer(this, ReportTag.KickByAnticheat, ReportTag.KickByPacket);
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
            if (meta is RpcMassMeta massMeta)
            {
                content += GenerateMassContent(timestamp, massMeta);
                return;
            }

            string target = AmongUsClient.Instance.FindObjectByNetId<PlayerControl>(meta.NetId)?.name ?? Players.GetAllPlayers().FirstOrDefault(p => p.NetId == meta.NetId)?.name;
            string recipient = Utils.PlayerByClientId(meta.Recipient).Map(p => p.name).OrElse("Unknown") + $" (Id: {meta.Recipient})";
            string rpc = ((RpcCalls)meta.CallId).Name();

            content += $"[{timestamp}] (Target: {target}, Recipient: {recipient}, RPC: {rpc}, Immediate: {meta.Immediate}, SendOptions: {meta.SendOption}, PacketSize: {meta.PacketSize}, Arguments: [{EnumerableExtensions.Fuse(meta.Arguments.Select(i => i?.ToString()?.RemoveHtmlTags()))}])\n";
        });
        return content;
    }

    private string GenerateMassContent(string timestamp, RpcMassMeta massMeta)
    {
        string content = "";
        string target = AmongUsClient.Instance.FindObjectByNetId<PlayerControl>(massMeta.NetId)?.name ?? Players.GetAllPlayers().FirstOrDefault(p => p.NetId == massMeta.NetId)?.name;
        string recipient = Utils.PlayerByClientId(massMeta.Recipient).Map(p => p.name).OrElse("Unknown") + $" (Id: {massMeta.Recipient})";

        content += $"[{timestamp}] MASS RPC => (Target: {target}, Recipient: {recipient}, Immediate: {massMeta.Immediate}, SendOptions: {massMeta.SendOption}, PacketSize: {massMeta.PacketSize})";
        massMeta.ChildMeta.ForEach(meta =>
        {
            string targ = AmongUsClient.Instance.FindObjectByNetId<PlayerControl>(meta.NetId)?.name ?? Players.GetAllPlayers().FirstOrDefault(p => p.NetId == meta.NetId)?.name;
            string recip = Utils.PlayerByClientId(meta.Recipient).Map(p => p.name).OrElse("Unknown") + $" (Id: {meta.Recipient})";
            string rpc = ((RpcCalls)meta.CallId).Name();

            content += $"- [{timestamp}] (Target: {targ}, Recipient: {recip}, RPC: {rpc}, Immediate: {meta.Immediate}, SendOptions: {meta.SendOption}, PacketSize: {meta.PacketSize}, Arguments: [{EnumerableExtensions.Fuse(meta.Arguments.Select(i => i?.ToString()?.RemoveHtmlTags()))}])\n";
        });
        return content;
    }
}