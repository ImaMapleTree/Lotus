using Lotus.Managers.Templates.Models.Backing;

namespace Lotus.Managers.Templates.Models.Units.Conditionals;

public class TConditionalEvaluate: CommonConditionalUnit
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(TConditionalEvaluate));

    private string inlineCondition;

    public TConditionalEvaluate(object input) : base(input)
    {
        string? parsed = input.ToString();
        if (parsed == null)
        {
            log.Warn("Error parsing \"Evaluate\" statement to string");
            parsed = "True == True";
        }

        inlineCondition = parsed;
    }

    public override bool Evaluate(object? data) => InlineConditionEvaluator.Evaluate(inlineCondition, data);
}