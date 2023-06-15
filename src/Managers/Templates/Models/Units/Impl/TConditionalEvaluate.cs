using Lotus.Managers.Templates.Models.Backing;
using VentLib.Logging;

namespace Lotus.Managers.Templates.Models.Units.Impl;

public class TConditionalEvaluate: CommonConditionalUnit
{
    private string inlineCondition;

    public TConditionalEvaluate(object input) : base(input)
    {
        string? parsed = input.ToString();
        if (parsed == null)
        {
            VentLogger.Warn("Error parsing \"Evaluate\" statement to string");
            parsed = "True == True";
        }

        inlineCondition = parsed;
    }

    public override bool Evaluate(object? data) => InlineConditionEvaluator.Evaluate(inlineCondition, data);
}