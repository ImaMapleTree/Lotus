using HarmonyLib;
using InnerNet;

namespace TOHTOR.Managers.Reporting;

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleDisconnect))]
public class AnticheatReporter
{
    private static void Postfix([HarmonyArgument(0)] DisconnectReasons reason)
    {
        //if (reason is not DisconnectReasons.Hacking) return;
        ReportManager.GenerateReport(ReportTag.KickByAnticheat);
    }
}