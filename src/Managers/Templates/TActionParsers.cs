using System;
using System.Collections.Generic;
using Lotus.Managers.Templates.Models.Units;
using Lotus.Managers.Templates.Models.Units.Actions;
using VentLib.Logging;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers.Templates;

public class TActionParsers
{
    public static Dictionary<string, Func<object, IActionUnit>> units = new()
    {
        { "Store", value => new TActionStore(value) },
        { "StoreAs", value => new TActionStoreAs(value) },

        { "Trigger", value => new TActionTrigger(value) },

        { "Add", value => new TActionAdd(value) },
        { "Subtract", value => new TActionSubtract(value) },
        { "Multiply", value => new TActionMultiply(value) },
        { "Divide", value => new TActionDivide(value) },
        { "Mod", value => new TActionMod(value) },

    };

    public static IActionUnit Parse(string yamlKey, object value)
    {
        return units.GetOptional(yamlKey).Map(c => c(value)).OrElseGet(() =>
        {
            VentLogger.Warn($"Could not find action for key: \"{yamlKey}\", returning default action.");
            return new TActionDefault();
        });
    }
}