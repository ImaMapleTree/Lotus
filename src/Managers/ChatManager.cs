using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Lotus.API.Odyssey;
using Lotus.Logging;
using Lotus.Managers.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lotus.Managers;

public class ChatManager
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(ChatManager));

    private readonly FileInfo filterFile;

    private IDeserializer deserializer = new DeserializerBuilder()
        .IgnoreUnmatchedProperties()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .Build();

    private ISerializer serializer = new SerializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .Build();


    private List<string> globalBannedWords;
    private List<string> lobbyBannedWords;


    public ChatManager(FileInfo filterFile)
    {
        this.filterFile = filterFile;
        try
        {
            BannedWordFile wordFile = Load();
            globalBannedWords = wordFile.GlobalBannedWords;
            lobbyBannedWords = wordFile.LobbyBannedWords;
        }
        catch (Exception e)
        {
            log.Exception("Error loading banned words list: ", e);
            globalBannedWords = new List<string>();
            lobbyBannedWords = new List<string>();
        }
    }

    public bool HasBannedWord(string message)
    {
        bool CheckBannedWord(string pattern) => Regex.IsMatch(message, pattern, RegexOptions.IgnoreCase);
        if (Game.State is GameState.InLobby && lobbyBannedWords.Any(CheckBannedWord)) return true;
        return globalBannedWords.Any(CheckBannedWord);
    }

    public string? Reload()
    {
        try
        {
            BannedWordFile wordFile = Load();
            globalBannedWords = wordFile.GlobalBannedWords;
            lobbyBannedWords = wordFile.LobbyBannedWords;
            return null;
        }
        catch (Exception exception)
        {
            log.Exception("Error loading banned words list", exception);
            return exception.ToString();
        }
    }

    private BannedWordFile Load()
    {
        BannedWordFile wordFile;
        if (!this.filterFile.Exists)
        {
            wordFile = new BannedWordFile();
            string yaml = serializer.Serialize(wordFile);
            FileStream writer = filterFile.Open(FileMode.Create);
            writer.Write(Encoding.UTF8.GetBytes(yaml));
            writer.Close();
            return wordFile;
        }

        string content;
        using (StreamReader reader = new(this.filterFile.Open(FileMode.Open))) content = reader.ReadToEnd();
        DevLogger.Log($"Content: {content}");
        return deserializer.Deserialize<BannedWordFile>(content);
    }
}