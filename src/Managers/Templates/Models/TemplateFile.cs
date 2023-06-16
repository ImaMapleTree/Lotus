using System.Collections.Generic;
using Lotus.Managers.Templates.Models.Units;

namespace Lotus.Managers.Templates.Models;

public class TemplateFile
{
    public Dictionary<string, TemplateUnit>? Variables { get; set; } = new();
    public List<Template>? Templates { get; set; } = new();
}