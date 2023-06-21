namespace Lotus.Managers.Templates.Models.Units.Actions;

public abstract class CommonActionUnit: IActionUnit
{
    protected object Input;

    public CommonActionUnit(object input)
    {
        Input = input.ToString() ?? "";
    }

    public abstract string Execute(string meta, object? data);
}