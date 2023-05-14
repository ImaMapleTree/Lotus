using UnityEngine;
using VentLib.Utilities;

namespace Lotus.GUI.Counters;

public class StaticCounter : ICounter
{
    private int type;
    private int topValue;
    private int botValue;
    private Color color = new(0.92f, 0.77f, 0.22f);

    public StaticCounter(int topValue, int botValue, Color? color = null)
    {
        if (color != null) this.color = color.Value;
        this.topValue = topValue;
        this.botValue = botValue;
        type = 1;
    }

    public StaticCounter(int topValue, Color? color = null)
    {
        if (color != null) this.color = color.Value;
        this.topValue = topValue;
    }

    public void Increment() => topValue++;

    public void Decrement() => botValue++;

    public string CountString()
    {
        string innerString = color.Colorize(type == 0 ? "" + topValue : $"{topValue}/{botValue}");
        return Color.white.Colorize($"({innerString})");
    }

    public int Count() => topValue;
}