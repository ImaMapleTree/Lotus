using System.Collections.Generic;

namespace TOHTOR.Managers.Templates;

public class TemplateData
{
    public string ActiveProfile { get; set; } = "general";
    public List<Template> Templates { get; set; } = new();
    public Dictionary<string, List<int>> Tags { get; set; } = new();
}