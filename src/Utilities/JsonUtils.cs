using System.IO;
using System.Text.Json;
using VentLib.Utilities.Optionals;

namespace Lotus.Utilities;

public class JsonUtils
{
    public static Optional<T> ReadJson<T>(FileInfo file, FileMode fileMode = FileMode.OpenOrCreate)
    {
        StreamReader reader = new(file.Open(fileMode));
        string text = reader.ReadToEnd();
        reader.Close();
        try
        {
            return Optional<T>.Of(JsonSerializer.Deserialize<T>(text));
        }
        catch
        {
            return Optional<T>.Null();
        }
    }

    public static Optional<T> ReadJson<T>(string path, FileMode fileMode = FileMode.OpenOrCreate) => ReadJson<T>(new FileInfo(path), fileMode);

    public static void WriteJson(object jsonObject, FileInfo file, FileMode fileMode = FileMode.Create)
    {
        StreamWriter writer = new(file.Open(fileMode));
        writer.Write(JsonSerializer.Serialize(jsonObject));
        writer.Close();
    }

    public static void WriteJson(object jsonObject, string path, FileMode fileMode = FileMode.Create)
    {
        WriteJson(jsonObject, new FileInfo(path), fileMode);
    }
}