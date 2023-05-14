using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Lotus.Managers.Friends;

public class LastKnownAs
{
    private Dictionary<string, string> dictionary;
    private FileInfo file;

    internal LastKnownAs(FileInfo fileInfo)
    {
        this.file = fileInfo;
        StreamReader reader = new(file.Open(FileMode.OpenOrCreate));
        try
        {
            dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd())!;
            reader.Close();
        }
        catch
        {
            dictionary = new Dictionary<string, string>();
            reader.Close();
            Save();
        }
    }
    
    public string? Name(string friendcode)
    {
        return dictionary.GetValueOrDefault(friendcode);
    }

    public string SetName(string friendcode, string name)
    {
        dictionary[friendcode] = name;
        Save();
        return name;
    }

    public void Save()
    {
        StreamWriter writer = new(file.Open(FileMode.Create));
        string text = JsonSerializer.Serialize(dictionary);
        writer.Write(text);
        writer.Close();
    }
}