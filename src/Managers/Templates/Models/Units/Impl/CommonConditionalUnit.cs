namespace Lotus.Managers.Templates.Models.Units.Impl;

public abstract class CommonConditionalUnit: IConditionalUnit
{
    protected object Input;

    public CommonConditionalUnit(object input)
    {
        Input = input;
    }

    public abstract bool Evaluate(object? data);
}