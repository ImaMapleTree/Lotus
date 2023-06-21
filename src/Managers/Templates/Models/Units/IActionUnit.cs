namespace Lotus.Managers.Templates.Models.Units;

public interface IActionUnit
{
    public string Execute(string meta, object? data);
}