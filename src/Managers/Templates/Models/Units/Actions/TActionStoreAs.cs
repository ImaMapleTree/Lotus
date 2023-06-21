using Lotus.Logging;

namespace Lotus.Managers.Templates.Models.Units.Actions;

public class TActionStoreAs: TActionStore
{
    public TActionStoreAs(object input) : base(input)
    {
    }

    public override string Execute(string meta, object? data)
    {
        string storedName = TemplateUnit.FormatStatic(Input.ToString() ?? "");
        DevLogger.Log($"Storing variable ({meta}) for: \"{storedName}\"");
        StoredVariables[storedName] = meta;
        return storedName;
    }
}