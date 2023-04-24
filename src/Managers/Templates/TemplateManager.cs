using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AmongUs.Data;
using HarmonyLib;
using InnerNet;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Utilities;
using VentLib.Options;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Managers.Templates;

public class TemplateManager
{

    private TemplateData templateData;

    internal List<Template> Templates;
    private readonly FileInfo templateFile;
    private readonly Regex regex = new("(?:\\$|@|%)((?:[A-Za-z0-9]|\\.\\S)*)");

    private Dictionary<string, List<Template>> taggedTemplates;

    public TemplateManager(FileInfo templateFile)
    {
        this.templateFile = templateFile;
        Reload();
    }

    public void Reload()
    {
        templateData = JsonUtils.ReadJson<TemplateData>(templateFile).OrElseGet(() => new TemplateData());
        SaveReload();
    }

    public void SaveReload()
    {
        Save();
        taggedTemplates = templateData.Tags.Select(kv =>
                (kv.Key, kv.Value
                    .Select(v => templateData.Templates[v])
                    .Where(t => t.Profiles.Contains(templateData.ActiveProfile))
                    .ToList()))
            .ToDict(tuple => tuple.Key, tuple => tuple.Item2);
    }

    public void Save() => JsonUtils.WriteJson(templateData, templateFile);

    public void SetProfile(string profile)
    {
        templateData.ActiveProfile = profile;
        SaveReload();
    }

    public string GetProfile() => templateData.ActiveProfile;

    public List<Template> GetAllTemplates() => templateData.Templates;

    public Template GetTemplate(int templateId) => templateData.Templates[templateId];

    public void TagTemplate(int templateId, string tag)
    {
        List<int> tags = templateData.Tags.GetOrCompute(tag, () => new List<int>());
        if (!tags.Contains(templateId)) tags.Add(templateId);
        SaveReload();
    }

    public void UntagTemplate(int templateId, string tag)
    {
        bool reload = templateData.Tags.GetOptional(tag).Map(tags => tags.RemoveAll(id => id == templateId) > 0).OrElse(false);
        if (reload) SaveReload();
    }

    public void DeleteTag(string tag)
    {
        if (templateData.Tags.Remove(tag)) SaveReload();
    }

    public void CreateTemplate(string text)
    {
        templateData.Templates.Add(new Template(text));
        SaveReload();
    }

    // Costly
    public void DeleteTemplate(int templateId)
    {
        Dictionary<Template, int> originalIds = templateData.Templates
            .Select((t, i) => (t, i))
            .ToDict(t => t.t, t => t.i);

        templateData.Templates.RemoveAt(templateId);

        Dictionary<int, int> remapping = originalIds
            .Select(kv => (kv.Value, templateData.Templates.IndexOf(kv.Key)))
            .ToDict(t => t.Value, t => t.Item2);

        templateData.Tags = templateData.Tags.ToDict(kv => kv.Key, kv => kv.Value.Select(i => remapping[i]).ToList());
        SaveReload();
    }

    public bool TryFormat(PlayerControl player, string tag, out string formatted)
    {
        formatted = "";
        if (!taggedTemplates.ContainsKey(tag)) return false;

        formatted = taggedTemplates[tag]
            .Select(template => template.Format(player).Replace("\\n", "\n"))
            .Join(delimiter: "\n\n");

        return true;
    }
}
