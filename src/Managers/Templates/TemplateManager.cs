using System.Collections.Generic;
using System.IO;
using System.Linq;
using VentLib.Logging;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Managers.Templates;

public class TemplateManager
{
    private FileInfo templateFile;
    private List<Template> templates;
    private Dictionary<string, Template> taggedTemplates;
    private Dictionary<string, string> registeredTags = new();

    internal TemplateManager(FileInfo templateFile)
    {
        this.templateFile = templateFile;
        LoadTemplates();
    }

    public int CreateTemplate(string template)
    {
        int id = templates.Count;
        templates.Add(new Template(template));
        SaveTemplates();
        return id;
    }

    public bool EditTemplate(int id, string template)
    {
        if (templates.Count <= id || id < 0) return false;
        templates[id].Text = template;
        SaveTemplates();
        return true;
    }

    public bool DeleteTemplate(int id)
    {
        if (templates.Count <= id || id < 0) return false;
        string? oldTag = templates[id].Tag;
        if (oldTag != null) taggedTemplates.Remove(oldTag);
        templates.RemoveAt(id);
        SaveTemplates();
        return true;
    }
    
    public bool TagTemplate(int id, string tag)
    {
        if (templates.Count <= id || id < 0) return false;
        string? oldTag = templates[id].Tag;
        if (oldTag != null) taggedTemplates.Remove(oldTag);
        Template? oldTemplate = taggedTemplates.GetValueOrDefault(tag);
        if (oldTemplate != null) oldTemplate.Tag = null;
        (taggedTemplates[tag] = templates[id]).Tag = tag;
        SaveTemplates();
        return true;
    }

    public bool UntagTemplate(int id)
    {
        if (templates.Count <= id || id < 0) return false;
        string? oldTag = templates[id].Tag;
        templates[id].Tag = null;
        if (oldTag != null) taggedTemplates.Remove(oldTag);
        SaveTemplates();
        return true;
    }

    public bool TryFormat(object obj, string tag, out string formatted)
    {
        formatted = "";
        if (!registeredTags.ContainsKey(tag)) VentLogger.Warn($"Tag \"{tag}\" is not registered. Please ensure all template tags have been registered through TemplateManager.RegisterTag().", "TemplateManager");
        if (!taggedTemplates.ContainsKey(tag)) return false;
        formatted = taggedTemplates[tag].Format(obj).Replace("\\n", "\n");
        return true;
    }

    public bool TryFormat(object obj, int id, out string formatted)
    {
        formatted = "";
        if (templates.Count <= id || id < 0) return false;
        formatted = templates[id].Format(obj).Replace("\\n", "\n");
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

    public List<Template> ListTemplates => templates.ToList();

    public List<(string tag, string description)> AllTags() => registeredTags.Select(rt => (rt.Key, rt.Value)).ToList();

    public void Reload() => LoadTemplates();
    
    private void LoadTemplates()
    {
        StreamReader reader = new StreamReader(templateFile.Open(FileMode.OpenOrCreate));
        string[] lines = reader.ReadToEnd().Split("\n").Where(l => l != "\n").Where(l => l != "").Select(l => l.Replace("\r", "")).ToArray();
        reader.Close();
        templates = lines.Select(l => new Template(l)).ToList();
        taggedTemplates = templates.Where(t => t.Tag != null).ToDict(t => t.Tag!, t => t)!;
    }

    private void SaveTemplates()
    {
        StreamWriter writer = new(templateFile.Open(FileMode.Create));
        templates.ForEach(t => writer.Write(t.Tag == null ? t.Text : $"{t.Tag} | {t.Text.Replace("|", "\\|")}\n"));
        writer.Close();
    }
    
    
}