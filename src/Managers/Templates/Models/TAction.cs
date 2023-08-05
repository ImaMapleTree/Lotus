using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.Managers.Templates.Models.Units;
using VentLib.Utilities.Extensions;
using static Lotus.Managers.Templates.TemplateManager;

namespace Lotus.Managers.Templates.Models;

public class TAction: Dictionary<string, object>
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(TAction));

    public static string MetaVariable = "";
    public string? Input { get; set; }
    public List<TCondition> Conditions { get; set; } = new();
    public List<TCondition> ConditionsAny { get; set; } = new();
    public List<IActionUnit> ParsedActions { get; set; }

    private string? unparsedInput;
    private bool fullInitialized;

    protected bool Evaluate(object? data) => Conditions.All(c => c.Evaluate(data)) && (ConditionsAny.IsEmpty() || ConditionsAny.Any(c => c.Evaluate(data)));

    public void Execute(string meta, object? data)
    {
        try
        {
            if (!fullInitialized) FullInitialize();

            MetaVariable = meta;
            if (unparsedInput != null) MetaVariable = TemplateUnit.FormatStatic(unparsedInput);

            if (Evaluate(data)) ParsedActions.ForEach(a => { MetaVariable = a.Execute(MetaVariable, data); });
        }
        catch (Exception exception)
        {
            log.Exception("Error running actions.", exception);
        }
    }

    private void FullInitialize()
    {
        if (TryGetValue(nameof(Input), out object? input))
            unparsedInput = input.ToString() ?? "";

        if (TryGetValue(nameof(Conditions), out object? conditions))
            Conditions = TemplateDeserializer.Deserialize<List<TCondition>>(TemplateSerializer.Serialize(conditions));

        if (TryGetValue(nameof(ConditionsAny), out object? conditionsAny))
            ConditionsAny = TemplateDeserializer.Deserialize<List<TCondition>>(TemplateSerializer.Serialize(conditionsAny));

        ParsedActions = this.Select(kv => TActionParsers.Parse(kv.Key, kv.Value)).ToList();
        fullInitialized = true;

    }
}