using System.Globalization;

namespace Lotus.Managers.Templates.Models.Units.Actions;

public class TActionMod: NumericActionUnit
{
    public TActionMod(object input) : base(input)
    {
    }

    public override string Execute(float meta, float operand, object? data)
    {
        return (meta % operand).ToString(CultureInfo.InvariantCulture);
    }
}