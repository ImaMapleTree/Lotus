using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TOHTOR.API;
using TOHTOR.GUI.Name.Impl;
using TOHTOR.GUI.Name.Interfaces;
using TOHTOR.Patches.Actions;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace TOHTOR.GUI.Name.Holders;

public class ComponentHolder<T> : RemoteList<T>, IComponentHolder<T> where T: INameModelComponent
{
    protected float Size = 2.925f;
    protected int DisplayLine;

    protected int Spacing = 0;

    private readonly Dictionary<byte, bool> updated = new();
    private readonly Dictionary<byte, string> cacheStates = new();

    public ComponentHolder()
    {
    }

    public ComponentHolder(int line = 0)
    {
        this.DisplayLine = line;
    }

    public RemoteList<T> Components() => this;

    public void SetSize(float size) => Size = size;

    public void SetLine(int line)
    {
        DisplayLine = line;
    }

    public int Line() => DisplayLine;

    public void SetSpacing(int spacing) => this.Spacing = spacing;

    public string Render(PlayerControl player, GameState state)
    {
        if (player.IsShapeshifted() && this is not NameHolder) return "";
        List<string> endString = new();
        ViewMode lastMode = ViewMode.Absolute;
        foreach (T component in this.Where(p => p.GameStates().Contains(state)).Where(p => p.Viewers().Any(pp => pp.PlayerId == player.PlayerId)))
        {
            ViewMode newMode = component.ViewMode();
            if (newMode is ViewMode.Replace or ViewMode.Absolute || lastMode is ViewMode.Overriden) endString.Clear();
            lastMode = newMode;
            endString.Add(component.GenerateText());
            if (newMode is ViewMode.Absolute) break;
        }

        string newString = endString.Join(delimiter: " ".Repeat(Spacing - 1));

        updated[player.PlayerId] = cacheStates.GetValueOrDefault(player.PlayerId, "") != newString;
        return cacheStates[player.PlayerId] = newString;
    }

    public bool Updated(byte playerId) => updated.GetValueOrDefault(playerId, false);
}