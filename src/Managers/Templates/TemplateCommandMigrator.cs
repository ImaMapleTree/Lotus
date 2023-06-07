using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Lotus.Chat;
using Lotus.Extensions;
using Lotus.Logging;
using Lotus.Managers.Templates.Models;
using VentLib.Logging;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers.Templates;

public class TemplateCommandMigrator
{
    private FileInfo file;
    private Dictionary<string, string> commandAliases = null!;
    private OrderedDictionary<string, List<string>> aliasDictionary = new();
    private static Regex _tagRegex = new("( *\\| *)");
    private static Regex _commaRegex = new("( *, *)");

    internal TemplateCommandMigrator(FileInfo file)
    {
        this.file = file;
        if (!file.Exists) return;
        Load();
        Dictionary<string, Template> commands = PluginDataManager.TemplateManager.Commands;
        bool addedAlias = false;
        commandAliases.ForEach(ca =>
        {
            Template? template = PluginDataManager.TemplateManager.GetTemplates(ca.Value)?.FirstOrDefault();
            if (template == null) return;
            template.Aliases ??= new List<string>();
            template.Aliases.Add(ca.Key);
            commands[ca.Key] = template;
            addedAlias = true;
        });
        if (addedAlias) PluginDataManager.TemplateManager.SaveTemplates();
        file.Rename("LEGACY_TemplateCommands.txt");
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
            try
            {
                string[] components = _tagRegex.Split(l);
                string tag = components[0];

                aliasDictionary[tag] = _commaRegex.Split(components[2]).Where(r => r is not (" " or "") && !r.Contains(',')).ToList();
                _commaRegex.Split(components[2]).Where(r => r is not (" " or "") && !r.Contains(',')).ForEach(a => commandAliases[a] = tag);
            }
            catch (Exception e)
            {
                VentLogger.Exception(e, "Could not parse template command!");
            }
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