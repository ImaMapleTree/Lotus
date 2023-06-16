using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lotus.Extensions;
using VentLib.Logging;

namespace Lotus.Managers.Templates;

public class TemplateMigration
{
    private readonly FileInfo lotusLegacyTemplateFile;
    private readonly FileInfo tohtorLegacyTemplateFile;

    private List<TemplateLegacy> templates;

    internal TemplateMigration(FileInfo lotusLegacyTemplateFile, FileInfo tohtorLegacyTemplateFile)
    {
        this.lotusLegacyTemplateFile = lotusLegacyTemplateFile;
        this.tohtorLegacyTemplateFile = tohtorLegacyTemplateFile;
        LoadTemplates();
        LoadTOHTemplates();
        if (templates == null! || templates.Count == 0) return;
        VentLogger.High($"Migrating {templates.Count} legacy templates to new system", "TemplateMigration");
        templates.ForEach(t =>
        {
            try
            {
                PluginDataManager.TemplateManager.AddTemplate(t.ConvertToTemplate());
            }
            catch (Exception exception)
            {
                VentLogger.Exception(exception, "Error migrating template.");
            }
        });
        VentLogger.High("Successfully migrated templates.", "TemplateMigration");
        if (this.lotusLegacyTemplateFile.Exists)
            lotusLegacyTemplateFile.Rename("LEGACY_LotusTemplates.txt", true);
        if (this.tohtorLegacyTemplateFile.Exists)
            tohtorLegacyTemplateFile.Rename("LEGACY_templates.txt", true);
    }

    private void LoadTemplates()
    {
        if (!lotusLegacyTemplateFile.Exists) return;
        StreamReader reader = new StreamReader(lotusLegacyTemplateFile.Open(FileMode.Open));
        string[] lines = reader.ReadToEnd().Split("\n").Where(l => l != "\n").Where(l => l != "").Select(l => l.Replace("\r", "")).ToArray();
        reader.Close();
        templates = lines.Select(l => new TemplateLegacy(l)).ToList();
    }

    private void LoadTOHTemplates()
    {
        if (!tohtorLegacyTemplateFile.Exists) return;
        StreamReader reader = new StreamReader(tohtorLegacyTemplateFile.Open(FileMode.Open));
        string[] lines = reader.ReadToEnd().Split("\n").Where(l => l != "\n").Where(l => l != "").Select(l => l.Replace("\r", "")).ToArray();
        reader.Close();
        if (templates == null!) templates = new List<TemplateLegacy>();
        templates.AddRange(lines.Select(TemplateLegacy.FromTOH).ToList());
    }


}