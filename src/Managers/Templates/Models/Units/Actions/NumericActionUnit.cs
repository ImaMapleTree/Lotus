using System;
using VentLib.Logging;

namespace Lotus.Managers.Templates.Models.Units.Actions;

public abstract class NumericActionUnit: CommonActionUnit
{
    public NumericActionUnit(object input) : base(input)
    {
    }

    public override string Execute(string meta, object? data)
    {
        return Execute(ParseFloatValue(TemplateUnit.FormatStatic(meta)), Convert.ToSingle(TemplateUnit.FormatStatic(Input.ToString() ?? "")), data);
    }

    public abstract string Execute(float meta, float operand, object? data);

    private static float ParseFloatValue(string input)
    {
        try
        {
            return float.Parse(input);
        }
        catch (Exception exception)
        {
            VentLogger.Exception(exception, $"Could not parse \"{input}\" to float.");
            return 0;
        }
    }


}