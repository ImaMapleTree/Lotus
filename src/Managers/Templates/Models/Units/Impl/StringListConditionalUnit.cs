using System.Collections;
using System.Collections.Generic;

namespace Lotus.Managers.Templates.Models.Units.Impl;

public abstract class StringListConditionalUnit: CommonConditionalUnit
{
    protected HashSet<string> Values = new();

    public StringListConditionalUnit(object input) : base(input)
    {
        if (Input is ICollection collection)
        {
            foreach (object o in collection)
            {
                string? value = o.ToString();
                if (value != null) Values.Add(value);
            }

            return;
        }

        string? inputValue = input.ToString();
        if (inputValue != null) Values.Add(inputValue);
    }
}