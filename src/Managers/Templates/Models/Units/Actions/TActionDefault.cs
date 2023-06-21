namespace Lotus.Managers.Templates.Models.Units.Actions;

public class TActionDefault: IActionUnit
{
    public string Execute(string meta, object? data)
    {
        return meta;
    }
}