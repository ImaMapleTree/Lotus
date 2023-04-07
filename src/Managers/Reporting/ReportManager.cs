using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using VentLib.Logging;
using VentLib.Options;
using VentLib.Options.IO;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Managers.Reporting;

[LoadStatic]
public static class ReportManager
{
    private static readonly DirectoryInfo ReportingDirectory;
    private static readonly Dictionary<ReportTag, OrderedSet<IReportProducer>> ReportProducers = new();
    private static readonly LogLevel ReportLevel = LogLevel.High.Similar("REPORT", ConsoleColor.Green);

    static ReportManager()
    {
        OptionManager reportingOptionManager = OptionManager.GetManager(file: "file_options.txt");
        var reportingDirectoryOption = new OptionBuilder().Name("Reporting Directory")
            .Description("The directory for storing various reports.\nDefault = reports")
            .Value("reports")
            .IOSettings(settings => settings.UnknownValueAction = ADEAnswer.Allow)
            .BuildAndRegister(reportingOptionManager);

        ReportingDirectory = new DirectoryInfo(reportingDirectoryOption.GetValue<string>());
        if (!ReportingDirectory.Exists) ReportingDirectory.Create();

        Enum.GetValues<ReportTag>().ForEach(t => ReportProducers.Add(t, new OrderedSet<IReportProducer>()));
    }

    public static void AddProducer(IReportProducer reportProducer, params ReportTag[] tags)
    {
        if (tags.Length == 0) tags = Enum.GetValues<ReportTag>();
        tags.ForEach(t => ReportProducers[t].Add(reportProducer));
    }

    public static void RemoveProducer(IReportProducer reportProducer)
    {
        ReportProducers.Values.ForEach(s => s.Remove(reportProducer));
    }

    public static void GenerateReport(params ReportTag[] tags)
    {
        HashSet<IReportProducer> exhaustedReporters = new();
        Dictionary<string, List<ReportInfo>> reports = new();

        if (tags.Length == 0) tags = Enum.GetValues<ReportTag>();
        tags.SelectMany(t => ReportProducers[t]).ForEach(p =>
        {
            if (!exhaustedReporters.Add(p)) return;
            ReportInfo reportInfo = p.ProduceReport();
            reports.GetOrCompute(reportInfo.Header.FileName, () => new List<ReportInfo>()).Add(reportInfo);
        });

        if (reports.Count == 0) return;
        VentLogger.Log(ReportLevel, $"Generating Report | Tags: [{tags.Join()}]");

        DirectoryInfo thisReport = ReportingDirectory.GetDirectory("latest");
        if (!thisReport.Exists) thisReport.Create();
        else
        {
            thisReport.Delete(true);
            thisReport.Create();
        }

        reports.ForEach(kp =>
        {
            string fileName = kp.Key;
            List<ReportInfo> reportInfos = kp.Value;
            FileInfo fileInfo = new(fileName.TrimEnd('.'));
            if (fileInfo.Extension == "") fileName += ".txt";

            fileInfo = thisReport.GetFile(fileName);

            string content = "";
            string fileContent = "";
            reportInfos.ForEach(i =>
            {
                string infoContent = $"======[{i.Header.ReportName}]======\n" + i.Content + "\n";
                content += infoContent;
                if (i.Header.SaveToFile) fileContent += infoContent;
            });

            VentLogger.Log(ReportLevel, $"\nReport for {fileName}\n{content}");
            StreamWriter writer = new(fileInfo.Open(FileMode.Create));
            writer.Write(fileContent);
            writer.Close();
        });
    }
}