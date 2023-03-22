using System;
using UnityEngine;
using VentLib.Utilities;

namespace TOHTOR.GUI.Name;

public struct LiveString
{
    public static LiveString Empty = new("");

    private readonly Color? mainColor = null;
    private readonly Func<string> valueSupplier;

    public LiveString(Func<string> supplier, Color? color = null)
    {
        mainColor = color;
        valueSupplier = supplier;
    }

    public LiveString(string value, Color? color = null) : this(() => value, color)
    {
    }

    public override string ToString()
    {
        return mainColor == null ? valueSupplier() : mainColor.Value.Colorize(valueSupplier());
    }
}