using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lotus.API.Player;
using Lotus.Managers.Templates.Models;
using VentLib.Logging;
using VentLib.Utilities.Extensions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lotus.Managers.Templates;

public class TemplateManager
{
    private IDeserializer deserializer = new DeserializerBuilder()
        .IgnoreUnmatchedProperties()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .Build();

    private ISerializer serializer = new SerializerBuilder()
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .Build();

    private FileInfo templateFileInfo;
    private TemplateFIle tFile;

    private Dictionary<string, List<Template>> templates = null!;
    private Dictionary<string, TemplateUnit>? variables;
    internal Dictionary<string, Template> Commands = null!;

    private List<Template>? allTemplates;
    private Exception? exception;

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
        template.Aliases?.ForEach(a => Commands[a] = template);

        if (template.Tag != null) templates.GetOrCompute(template.Tag, () => new List<Template>()).Add(template);
        SaveTemplates();
    }

    internal bool CheckAndRunCommand(PlayerControl source, string input)
    {
        input = input.TrimStart('/');
        Template? template = Commands.GetValueOrDefault(input);
        if (template == null) return false;
        template.SendMessage(source, source);
        return true;
    }

    public int CreateTemplate(string template)
    {
        int id = allTemplates!.Count;
        allTemplates.Add(new Template(template));
        SaveTemplates();
        return id;
    }

    public bool DeleteTemplate(int id)
    {
        if (allTemplates == null) return false;
        Template oldTemplate = allTemplates[id];
        allTemplates.RemoveAt(id);
        if (oldTemplate.Tag != null) templates.GetValueOrDefault(oldTemplate.Tag)?.Remove(oldTemplate);
        SaveTemplates();
        return true;
    }

    public bool EditTemplate(int id, string template)
    {
        if (allTemplates == null || allTemplates.Count <= id || id < 0) return false;
        allTemplates[id].Text = template;
        SaveTemplates();
        return true;
    }

    public string? FormatVariable(string variable, PlayerControl? user, object? obj)
    {
        if (variables == null) return null;
        return variables.TryGetValue(variable, out TemplateUnit? templateUnit) ? templateUnit.Format(user, obj) : null;
    }

    public bool Preview(PlayerControl? viewer, int id)
    {
        if (allTemplates == null || id < 0 || id >= allTemplates.Count) return false;
        allTemplates?[id].SendMessage(viewer!, viewer, viewer);
        return true;
    }

    public bool ShowAll(string tag, PlayerControl source, object? obj = null) => ShowAll(tag, source, Players.GetAllPlayers(), obj);

    public bool ShowAll(string tag, PlayerControl source, IEnumerable<PlayerControl> viewers, object? obj = null)
    {
        List<Template>? templs = GetTemplates(tag);
        if (templs == null) return false;
        templs.ForEach(t => viewers.ForEach(v => t.SendMessage(source, v, obj)));
        return true;
    }

    public bool TryFormat(PlayerControl? viewer, object? obj, string tag, out string formatted, bool ignoreWarning = false)
    {
        formatted = "";
        if (!ignoreWarning && !registeredTags.ContainsKey(tag)) VentLogger.Warn($"Tag \"{tag}\" is not registered. Please ensure all template tags have been registered through TemplateManager.RegisterTag().", "TemplateManager");
        if (!templates!.ContainsKey(tag)) return false;
        formatted = templates.GetValueOrDefault(tag)?.Select(v => v.Format(viewer, obj).Replace("\\n", "\n")).Where(t => t != "").Fuse("\n") ?? "";
        return true;
    }

    public bool TagTemplate(int id, string tag)
    {
        if (allTemplates == null || allTemplates.Count <= id || id < 0) return false;
        Template template = allTemplates[id];
        if (template.Tag != null) templates!.Remove(template.Tag);
        template.Tag = tag;
        templates!.GetOrCompute(tag, () => new List<Template>()).Add(template);
        SaveTemplates();
        return true;
    }

    public bool UntagTemplate(int id)
    {
        if (allTemplates == null || allTemplates.Count <= id || id < 0) return false;
        Template template = allTemplates[id];
        if (template.Tag != null) templates?.GetValueOrDefault(template.Tag)?.Remove(template);
        template.Tag = null;
        SaveTemplates();
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
        Commands = new Dictionary<string, Template>();
        templates = new Dictionary<string, List<Template>>();
        try
        {
            string result;
            using (StreamReader reader = new(templateFileInfo.Open(FileMode.OpenOrCreate))) result = reader.ReadToEnd();
            tFile = deserializer.Deserialize<TemplateFIle>(result);

            allTemplates = tFile.Templates;
            allTemplates?.Where(kv => kv.Tag != null).ForEach(kv => templates.GetOrCompute(kv.Tag!, () => new List<Template>()).Add(kv));
            variables = tFile.Variables;
            allTemplates?.Where(t => t.Aliases != null).ForEach(t => t.Aliases!.ForEach(a => Commands[a] = t));
            return null;
        }
        catch (Exception e)
        {
            this.exception = e;
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