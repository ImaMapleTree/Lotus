namespace Lotus.Managers.Templates.Models.Units;

public interface IConditionalUnit
{
    public bool Evaluate(object? data);
}