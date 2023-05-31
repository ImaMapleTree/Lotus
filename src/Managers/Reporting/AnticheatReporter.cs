using HarmonyLib;
using InnerNet;
using VentLib.Logging;

namespace Lotus.Managers.Reporting;

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleDisconnect))]
public class AnticheatReporter
{
    private static void Postfix(DisconnectReasons reason, string stringReason)
    {
        switch (reason)
        {
            case DisconnectReasons.Hacking:
                ReportManager.GenerateReport(ReportTag.KickByAnticheat);
                break;
            case DisconnectReasons.Custom:
                if (stringReason.Contains("Reliable packet")) ReportManager.GenerateReport(ReportTag.KickByPacket);
                break;
        }
    }
}