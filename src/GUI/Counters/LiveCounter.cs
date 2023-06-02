using System;
using UnityEngine;
using VentLib.Utilities;

namespace Lotus.GUI.Counters;

public class LiveCounter : ICounter
{
    private int type;
    private Func<int> topSupplier;
    private Func<int>? botSupplier;
    private Color color = new(0.92f, 0.77f, 0.22f);

    public LiveCounter(Func<int> topSupplier, Func<int> botSupplier, Color? color = null)
    {
        if (color != null) this.color = color.Value;
        this.topSupplier = topSupplier;
        this.botSupplier = botSupplier;
        type = 1;
    }

    public LiveCounter(Func<int> topSupplier, Color? color = null)
    {
        if (color != null) this.color = color.Value;
        this.topSupplier = topSupplier;
    }

    public string CountString()
    {
        string innerString = color.Colorize(type == 0 ? "" + topSupplier() : $"{topSupplier()}/{botSupplier!()}");
        return Color.white.Colorize($"({innerString})");
    }

    public int Count() => topSupplier();
}