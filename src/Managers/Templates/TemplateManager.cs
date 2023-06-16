﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lotus.API.Player;
using Lotus.Logging;
using Lotus.Managers.Hotkeys;
using Lotus.Managers.Templates.Models;
using Lotus.Managers.Templates.Models.Units;
using VentLib.Logging;
using VentLib.Utilities.Extensions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lotus.Managers.Templates;

public class TemplateManager
{
    public static int GlobalTriggerCount;

    private IDeserializer deserializer = new DeserializerBuilder()
        .IgnoreUnmatchedProperties()
        .WithDuplicateKeyChecking()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .Build();

    private ISerializer serializer = new SerializerBuilder()
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .Build();

    private FileInfo templateFileInfo;
    private TemplateFile tFile;

    private Dictionary<string, List<Template>> templates = null!;
    private Dictionary<string, TemplateUnit>? variables;
    internal Dictionary<string, List<Template>> Commands = null!;

    private List<Template>? allTemplates;

    private Dictionary<string, string> registeredTags = new()
    {
        {"modifier-info", "The template is used by the @ModsDescriptive tag when displaying modifiers. This template uses ^Role_XXX variables to display its information."}
    };

    internal TemplateManager(FileInfo templateFileInfo)
    {
        this.templateFileInfo = templateFileInfo;
        LoadTemplates();
    }

    public List<Template> ListTemplates => allTemplates!.ToList();

    public List<(string tag, string description)> AllTags() => registeredTags.Select(rt => (rt.Key, rt.Value)).ToList();

    internal void AddTemplate(Template template)
    {
        allTemplates?.Add(template);
        template.Aliases?.ForEach(a => Commands.GetOrCompute(a, () => new List<Template>()).Add(template));

        if (template.Tag != null) templates.GetOrCompute(template.Tag, () => new List<Template>()).Add(template);
        SaveTemplates();
    }

    internal bool CheckAndRunCommand(PlayerControl source, string input)
    {
        if (input.Length < 2 || !input.StartsWith("/")) return false;
        input = input[1..];
        List<Template>? templates = Commands.GetValueOrDefault(input);
        if (templates == null) return false;
        templates.ForEach(template =>
        {
            if (source.IsHost())
            {
                DevLogger.Log(input);
                if (HotkeyManager.HoldingRightShift) ShowAll(template, source, Players.GetPlayers(PlayerFilter.Dead));
                else ShowAll(template, source);
            }
            else template.SendMessage(source, source);
        });
        return true;
    }

    public string? FormatVariable(string variable, object? obj)
    {
        if (variables == null) return null;
        return variables.TryGetValue(variable, out TemplateUnit? templateUnit) ? templateUnit.Format(obj) : null;
    }

    public bool ShowAll(string tag, PlayerControl source, object? obj = null) => ShowAll(tag, source, Players.GetPlayers(), obj);

    public bool ShowAll(string tag, PlayerControl source, IEnumerable<PlayerControl> viewers, object? obj = null)
    {
        List<Template>? templs = GetTemplates(tag);
        return templs != null && ShowAll(templs, source, viewers, obj);
    }

    public bool ShowAll(Template template, PlayerControl source, object? obj = null) => ShowAll(template, source, Players.GetPlayers(), obj);

    public bool ShowAll(Template template, PlayerControl source, IEnumerable<PlayerControl> viewers, object? obj = null)
    {
        return ShowAll(new List<Template> { template }, source, viewers, obj);
    }

    public bool ShowAll(List<Template> template, PlayerControl source, IEnumerable<PlayerControl> viewers, object? obj = null)
    {
        template.ForEach(t => viewers.ForEach(v => t.SendMessage(source, v, obj)));
        return true;
    }

    public bool TryFormat(object? obj, string tag, out string formatted, bool ignoreWarning = false)
    {
        formatted = "";
        if (!ignoreWarning && !registeredTags.ContainsKey(tag)) VentLogger.Warn($"Tag \"{tag}\" is not registered. Please ensure all template tags have been registered through TemplateManager.RegisterTag().", "TemplateManager");
        if (!templates!.ContainsKey(tag)) return false;
        formatted = templates.GetValueOrDefault(tag)?.Select(v => v.Format(obj).Replace("\\n", "\n")).Where(t => t != "").Fuse("\n") ?? "";
        return true;
    }

    public bool RegisterTag(string tag, string description)
    {
        if (registeredTags.ContainsKey(tag) && registeredTags[tag] != description)
        {
            VentLogger.Warn($"Could not register template tag \"{tag}\". A tag of the same name already exists.", "TemplateManager");
            return false;
        }

        registeredTags[tag] = description;
        return true;
    }

    public bool HasTemplate(string tag) => templates?.ContainsKey(tag) ?? false;

    public List<Template>? GetTemplates(string tag) => templates?.GetValueOrDefault(tag);

    public string? LoadTemplates()
    {
        GlobalTriggerCount = 0;
        TTrigger.BoundHooks.ForEach(kv => kv.Value.Unbind(kv.Key));
        TTrigger.BoundHooks.Clear();

        Commands = new Dictionary<string, List<Template>>();
        templates = new Dictionary<string, List<Template>>();
        try
        {
            string result;
            using (StreamReader reader = new(templateFileInfo.Open(FileMode.OpenOrCreate))) result = reader.ReadToEnd();
            if (result == null!) tFile = new TemplateFile();
            else tFile = deserializer.Deserialize<TemplateFile>(result);
            if (tFile == null!) tFile = new TemplateFile();

            allTemplates = tFile.Templates;
            allTemplates?.Where(kv =>
            {
                kv.Setup();
                return kv.Tag != null;
            }).ForEach(kv => templates.GetOrCompute(kv.Tag!, () => new List<Template>()).Add(kv));

            variables = tFile.Variables;
            allTemplates?.Where(t => t.Aliases != null).ForEach(t => t.Aliases!.ForEach(a => Commands.GetOrCompute(a, () => new List<Template>()).Add(t)));
            return null;
        }
        catch (Exception e)
        {
            VentLogger.Exception(e);
            return e.ToString();
        }
    }

    public void SaveTemplates()
    {
        string yaml = serializer.Serialize(tFile);
        using FileStream stream = templateFileInfo.Open(FileMode.Create);
        stream.Write(Encoding.UTF8.GetBytes(yaml));
    }
}