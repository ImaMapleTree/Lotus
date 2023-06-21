using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.GUI.Name.Interfaces;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.GUI.Name.Holders;

public class ComponentHolder<T> : RemoteList<T>, IComponentHolder<T> where T: INameModelComponent
{
    protected float Size = 2.925f;
    protected int DisplayLine;
    protected int Spacing = 0;

    protected readonly Dictionary<byte, bool> updated = new();
    protected readonly Dictionary<byte, string> CacheStates = new();
    private readonly List<Action<INameModelComponent>> eventConsumers = new();

    public ComponentHolder()
    {
    }

    public ComponentHolder(int line = 0)
    {
        this.DisplayLine = line;
    }

    public RemoteList<T> Components() => this;

    public void SetSize(float size) => Size = size;

    public void SetLine(int line) => DisplayLine = line;

    public int Line() => DisplayLine;

    public void SetSpacing(int spacing) => this.Spacing = spacing;

    public virtual string Render(PlayerControl player, GameState state)
    {
        List<string> endString = new();
        ViewMode lastMode = ViewMode.Absolute;
        // TODO: if laggy investigate ways to circumvent ToArray() call here
        foreach (T component in this.Where(p => p.GameStates().Contains(state)).ToArray().Where(p => p.Viewers().Any(pp => pp.PlayerId == player.PlayerId)))
        {
            ViewMode newMode = component.ViewMode();
            if (newMode is ViewMode.Replace or ViewMode.Absolute || lastMode is ViewMode.Overriden) endString.Clear();
            lastMode = newMode;
            string text = component.GenerateText();
            if (text == null) continue;
            endString.Add(text);
            if (newMode is ViewMode.Absolute) break;
        }

        string newString = endString.Join(delimiter: " ".Repeat(Spacing - 1));

        updated[player.PlayerId] = CacheStates.GetValueOrDefault(player.PlayerId, "") != newString;
        return CacheStates[player.PlayerId] = newString;
    }

    public bool Updated(byte playerId) => updated.GetValueOrDefault(playerId, false);

    public new virtual Remote<T> Add(T component)
    {
        Remote<T> remote = base.Add(component);
        eventConsumers.ForEach(ev => ev(component));
        return remote;
    }

    public void AddListener(Action<INameModelComponent> eventConsumer) => eventConsumers.Add(eventConsumer);
}