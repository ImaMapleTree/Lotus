namespace Lotus.Managers.Reporting;

public class ReportInfo
{
    internal readonly ReportHeader Header;
    internal readonly string Content;

    private ReportInfo(ReportHeader header, string content)
    {
        this.Header = header;
        this.Content = content;
    }

    public static ReportHeader Create(string reportName, string? fileName = null, bool saveToFile = true)
    {
        return new ReportHeader(reportName, fileName, saveToFile);
    }

    public struct ReportHeader
    {
        public string ReportName { get; }
        public string FileName { get; }
        public bool SaveToFile { get; }

        internal ReportHeader(string reportName, string? fileName = null, bool saveToFile = true)
        {
            ReportName = reportName;
            FileName = (fileName ?? reportName).Replace(" ", "-").ToLower();
            SaveToFile = saveToFile;
        }

        public ReportInfo Attach(string content)
        {
            return new ReportInfo(this, content);
        }
    }
}