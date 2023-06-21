using System.Globalization;

namespace Lotus.Managers.Templates.Models.Units.Actions;

public class TActionDivide: NumericActionUnit
{
    public TActionDivide(object input) : base(input)
    {
    }

    public override string Execute(float meta, float operand, object? data)
    {
        return (meta / operand).ToString(CultureInfo.InvariantCulture);
    }
}