using System.Collections.Generic;

namespace Lotus.Managers.Templates.Models.Units.Actions;

public class TActionStore: CommonActionUnit
{
    public static readonly Dictionary<string, string> StoredVariables = new();

    public TActionStore(object input) : base(input)
    {
    }

    public override string Execute(string meta, object? data)
    {
        StoredVariables[TemplateUnit.FormatStatic(Input.ToString() ?? "")] = meta;
        return meta;
    }
}