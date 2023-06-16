using System.IO;
using VentLib.Utilities.Extensions;

namespace Lotus.Extensions;

public static class FileInfoExtensions
{
    public static void Rename(this FileInfo fileInfo, string name, bool overwrite = false)
    {
        DirectoryInfo directory = fileInfo.Directory!;
        fileInfo.MoveTo(directory.GetFile(name).FullName, overwrite);
    }
}