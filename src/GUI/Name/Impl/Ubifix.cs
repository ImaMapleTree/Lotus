using System.Collections.Generic;
using TOHTOR.GUI.Name.Interfaces;
using UnityEngine;

namespace TOHTOR.GUI.Name.Impl;

public class Ubifix
{
    private List<(INameModelComponent, int)> fixes = new();
    private LiveString liveString;


    public Ubifix(LiveString liveString)
    {
        this.liveString = liveString;
    }

    public Ubifix(string str, Color color) : this(new LiveString(str, color))
    {
    }

    public void AddComponent(INameModelComponent nameModelComponent, int fix) => fixes.Add((nameModelComponent, fix));

    public void Delete() => fixes.ForEach(f =>
    {
        if (f.Item2 == 0) f.Item1.RemovePrefix(this);
        if (f.Item2 == 1) f.Item1.RemoveSuffix(this);
    });

    public override string ToString() => liveString.ToString();
}