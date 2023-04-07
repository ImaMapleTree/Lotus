using VentLib.Utilities.Debug.Profiling;

namespace TOHTOR.Managers.Reporting;

public class RProfiler : Profiler, IReportProducer
{
    public RProfiler(string name) : base(name)
    {
    }


    public ReportInfo ProduceReport() => ReportInfo.Create($"Profiler \"{Name}\" Report", $"profiler-{Name}-report").Attach(GetCurrentData().ToString());

    public void HandleSignal(ReportSignal signal)
    {
    }
}