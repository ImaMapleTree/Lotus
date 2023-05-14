using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.GUI.Name.Impl;
using Lotus.GUI.Name.Interfaces;
using Lotus.API;
using VentLib.Utilities.Optionals;

namespace Lotus.GUI.Name.Components;

public class SimpleComponent : INameModelComponent
{
    private List<Ubifix> prefixes = new();
    private List<Ubifix> suffixes = new();

    private LiveString mainText;
    private Func<List<PlayerControl>> viewers;
    private readonly List<PlayerControl> additionalViewers = new();
    private ViewMode viewMode;
    private GameState[] gameStates;

    private Optional<float> size = Optional<float>.Null();

    private SimpleComponent() {}

    protected SimpleComponent(LiveString mainText, GameState[] gameStates, ViewMode viewMode = Name.ViewMode.Additive,
        Func<List<PlayerControl>>? viewers = null)
    {
        this.viewers = viewers;
        List<PlayerControl> allViewers = PlayerControl.AllPlayerControls.ToArray().ToList();
        this.viewers ??= () => allViewers;
        this.mainText = mainText;
        this.gameStates = gameStates;
        this.viewMode = viewMode;
    }

    protected SimpleComponent(LiveString mainText, GameState[] gameStates, ViewMode viewMode = Name.ViewMode.Additive,
        params PlayerControl[] viewers)
    {
        List<PlayerControl> allViewers = new();
        this.viewers = () => allViewers;
        this.mainText = mainText;
        this.gameStates = gameStates;
        this.viewMode = viewMode;
        additionalViewers = (viewers.Length > 0 ? viewers : PlayerControl.AllPlayerControls.ToArray()).ToList();
    }

    protected SimpleComponent(LiveString mainText, GameState gameState, ViewMode viewMode = Name.ViewMode.Additive,
        params PlayerControl[] viewers) : this(mainText, new[] { gameState }, viewMode, viewers)
    {
    }

    protected SimpleComponent(string mainText, GameState[] gameStates, ViewMode viewMode = Name.ViewMode.Additive,
        params PlayerControl[] viewers)
        : this(new LiveString(mainText), gameStates, viewMode, viewers)
    {
    }

    public void AddPrefix(Ubifix prefix)
    {
        prefixes.Add(prefix);
        prefix.AddComponent(this, 0);
    }

    public void AddSuffix(Ubifix suffix)
    {
        prefixes.Add(suffix);
        suffix.AddComponent(this, 1);
    }

    public void RemovePrefix(Ubifix prefix) => prefixes.Remove(prefix);

    public void RemoveSuffix(Ubifix suffix) => suffixes.Remove(suffix);

    public GameState[] GameStates() => gameStates;

    public Optional<float> Size() => size;

    public List<PlayerControl> Viewers() => viewers().Concat(additionalViewers).ToList();

    public void SetMainText(LiveString liveString) => mainText = liveString;

    public void SetViewerSupplier(Func<List<PlayerControl>> viewers) => this.viewers = viewers;

    public void AddViewer(PlayerControl player) => additionalViewers.Add(player);

    public void RemoveViewer(byte playerId) => additionalViewers.RemoveAll(v => v.PlayerId == playerId);

    public string GenerateText()
    {
        string newString = prefixes.Join(delimiter: "") + mainText + suffixes.Join(delimiter: "");
        size.IfPresent(s => newString = TextUtils.ApplySize(s, newString));
        return newString;
    }

    public ViewMode ViewMode() => viewMode;

    public INameModelComponent Clone()
    {
        List<PlayerControl> newViewerList = new(viewers());
        SimpleComponent component = new()
        {
            prefixes = new List<Ubifix>(prefixes),
            suffixes = new List<Ubifix>(suffixes),
            mainText = mainText,
            viewers = () => newViewerList,
            viewMode = viewMode,
            gameStates = gameStates,
            size = Optional<float>.From(size)
        };
        component.prefixes.ForEach(p => p.AddComponent(component, 0));
        component.suffixes.ForEach(p => p.AddComponent(component, 1));
        return component;
    }
}