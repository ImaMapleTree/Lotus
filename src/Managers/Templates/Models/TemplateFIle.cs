using System.Collections.Generic;

namespace Lotus.Managers.Templates.Models;

public class TemplateFIle
{
    public Dictionary<string, TemplateUnit>? Variables { get; set; }
    public List<Template>? Templates { get; set; }
}