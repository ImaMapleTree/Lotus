using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using VentLib.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lotus.Managers.Templates.Models.Units.Conditionals;

public class TRecursiveCondition: CommonConditionalUnit
{
    private IDeserializer deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();
    private ISerializer serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

    private List<TCondition> conditions = new();

    public TRecursiveCondition(object input) : base(input)
    {
        try
        {
            TCondition? condition = deserializer.Deserialize<TCondition>(serializer.Serialize(input));
            if (condition == null) throw new Exception();
            conditions.Add(condition);
        }
        catch (Exception exception)
        {
            try
            {
                List<TCondition>? parsedConditions =
                    deserializer.Deserialize<List<TCondition>>(serializer.Serialize(input));
                if (parsedConditions == null) throw new DataException($"Could not parse \"{input}\" as a valid type of {nameof(TCondition)}");
                conditions.AddRange(parsedConditions);
            }
            catch(Exception ex)
            {
                VentLogger.Exception(ex, "Parsing failed");
            }
        }
    }

    public override bool Evaluate(object? data) => conditions.All(c => c.Evaluate(data));
}