using System.IO;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers;

public class DataFileAccessor
{
    protected FileInfo SourceFile;
    protected string SourceText;

    public DataFileAccessor(FileInfo sourceFileInfo)
    {
        SourceFile = sourceFileInfo;
        SourceText = SourceFile.ReadAll(!SourceFile.Exists);
    }

    public DataFileAccessor(string path) : this(new FileInfo(path))
    {
    }
}