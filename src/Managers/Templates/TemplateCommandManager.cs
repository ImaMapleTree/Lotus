using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Lotus.Chat;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers.Templates;

public class TemplateCommandManager
{
    private FileInfo file;
    private Dictionary<string, string> commandAliases = null!;
    private OrderedDictionary<string, List<string>> aliasDictionary = new();
    private static Regex _tagRegex = new("( *\\| *)");
    private static Regex _commaRegex = new("( *, *)");
    
    internal TemplateCommandManager(FileInfo file)
    {
        this.file = file;
        Load();
    }

    public ResultType Create(string tag, List<string> aliases)
    {
        if (aliasDictionary.ContainsKey(tag)) return ResultType.TagAlreadyExists;
        aliasDictionary[tag] = aliases;
        aliases.ForEach(a => commandAliases[a] = tag);
        Save();
        return ResultType.Success;
    }

    public ResultType Delete(string tag)
    {
        List<string> aliases = aliasDictionary.GetOptional(tag).OrElse(null!);
        if (aliases == null!) return ResultType.TagDoesNotExist;

        aliasDictionary.Remove(tag);
        aliases.ForEach(a => commandAliases.Remove(a));
        Save();
        return ResultType.Success;
    }

    public ResultType AddAlias(string tag, string alias)
    {
        if (!commandAliases.TryAdd(alias, tag)) return ResultType.AliasAlreadyExists;
        List<string> aliases = aliasDictionary.GetOptional(tag).OrElse(null!);
        if (aliases == null!) return ResultType.TagDoesNotExist;
        aliases.Add(alias);
        Save();
        return ResultType.Success;
    }

    public ResultType RemoveAlias(string tag, string alias)
    {
        if (!commandAliases.ContainsKey(alias)) return ResultType.AliasDoesNotExist;
        commandAliases.Remove(alias);
        
        List<string> aliases = aliasDictionary.GetOptional(tag).OrElse(null!);
        if (aliases == null!) return ResultType.TagDoesNotExist;
        aliases.Remove(alias);
        Save();
        return ResultType.Success;
    }

    public List<(string tags, List<string> aliases)> ListAll()
    {
        return aliasDictionary.Select(kvp => (kvp.Key, kvp.Value)).ToList();
    }

    internal bool CheckAndRunCommand(PlayerControl source, string input)
    {
        input = input.TrimStart('/');
        string? tag = commandAliases.GetValueOrDefault(input);
        if (tag == null) return false;
        if (!PluginDataManager.TemplateManager.TryFormat(source, tag, out string formatted)) return true;
        ChatHandler.Of(formatted).LeftAlign().Send(source);
        return true;
    }

    private void Save()
    {
        StreamWriter writer = new(file.Open(FileMode.Create));
        string content = aliasDictionary.Select(kv => $"{kv.Key} | {kv.Value.Fuse()}").Fuse("\n");
        writer.Write(content);
        writer.Close();
    }

    private void Load()
    {
        commandAliases = new Dictionary<string, string>();
        aliasDictionary = new OrderedDictionary<string, List<string>>();
        StreamReader reader = new(file.Open(FileMode.OpenOrCreate));
        string[] lines = reader.ReadToEnd().Split("\n").Where(l => l is not ("\n" or "")).Select(l => l.Replace("\r", "")).ToArray();
        reader.Close();
        lines.Where(l => l.Contains('|')).ForEach(l =>
        {
            string[] components = _tagRegex.Split(l);
            string tag = components[0];
            aliasDictionary[tag] = components.ToList();
            _commaRegex.Split(components[1]).Where(r => r is not (" " or "")).ForEach(a => commandAliases[a] = tag);
        });
    }
}

public enum ResultType
{
    AliasDoesNotExist,
    AliasAlreadyExists,
    TagDoesNotExist,
    TagAlreadyExists,
    Success
}