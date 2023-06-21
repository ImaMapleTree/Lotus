using System.Collections.Generic;
using System.Linq;
using Lotus.Managers.Templates.Models.Units;

namespace Lotus.Managers.Templates.Models;

// ReSharper disable once InconsistentNaming
public class TCondition: Dictionary<string, object>
{
    public List<IConditionalUnit>? ParsedConditions;
    public string? Fallback { get; set; }

    public bool Evaluate(object? data)
    {
        ParsedConditions ??= this.Select(kv => TConditionalParsers.Parse(kv.Key, kv.Value)).ToList();
        return ParsedConditions.All(c => c.Evaluate(data));
    }
}