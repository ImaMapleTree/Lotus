namespace Lotus.Managers.Templates.Models.Units.Actions;

public class TActionTrigger: CommonActionUnit
{
    public TActionTrigger(object input) : base(input)
    {
    }

    public override string Execute(string meta, object? data)
    {
        PluginDataManager.TemplateManager.ShowAll(TemplateUnit.FormatStatic(Input.ToString() ?? ""), null!, data);
        return meta;
    }
}