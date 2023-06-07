using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.GUI.Name.Impl;
using Lotus.GUI.Name.Interfaces;
using Lotus.Options;
using VentLib.Utilities.Optionals;

namespace Lotus.GUI.Name.Components;

public class SimpleComponent : INameModelComponent
{
    protected List<Ubifix> Prefixes = new();
    protected List<Ubifix> Suffixes = new();

    protected LiveString MainText;
    private Func<List<PlayerControl>> viewers;
    private readonly List<PlayerControl> additionalViewers = GeneralOptions.GameplayOptions.GhostsSeeInfo ? Game.GetDeadPlayers().ToList() : new List<PlayerControl>();
    private ViewMode viewMode;
    private GameState[] gameStates;

    protected Optional<float> size = Optional<float>.Null();

    private SimpleComponent() {}

    protected SimpleComponent(LiveString mainText, GameState[] gameStates, ViewMode viewMode = Name.ViewMode.Additive,
        Func<List<PlayerControl>>? viewers = null)
    {
        this.viewers = viewers;
        List<PlayerControl> allViewers = PlayerControl.AllPlayerControls.ToArray().ToList();
        this.viewers ??= () => allViewers;
        this.MainText = mainText;
        this.gameStates = gameStates;
        this.viewMode = viewMode;
    }

    protected SimpleComponent(LiveString mainText, GameState[] gameStates, ViewMode viewMode = Name.ViewMode.Additive,
        params PlayerControl[] viewers)
    {
        List<PlayerControl> allViewers = new();
        this.viewers = () => allViewers;
        this.MainText = mainText;
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
        Prefixes.Add(prefix);
        prefix.AddComponent(this, 0);
    }

    public void AddSuffix(Ubifix suffix)
    {
        Prefixes.Add(suffix);
        suffix.AddComponent(this, 1);
    }

    public void RemovePrefix(Ubifix prefix) => Prefixes.Remove(prefix);

    public void RemoveSuffix(Ubifix suffix) => Suffixes.Remove(suffix);

    public GameState[] GameStates() => gameStates;

    public Optional<float> Size() => size;

    public List<PlayerControl> Viewers() => viewers().Concat(additionalViewers).ToList();

    public void SetMainText(LiveString liveString) => MainText = liveString;

    public void SetViewerSupplier(Func<List<PlayerControl>> viewers) => this.viewers = viewers;

    public void AddViewer(PlayerControl player) => additionalViewers.Add(player);

    public void RemoveViewer(byte playerId) => additionalViewers.RemoveAll(v => v.PlayerId == playerId);

    public virtual string GenerateText()
    {
        string newString = Prefixes.Join(delimiter: "") + MainText + Suffixes.Join(delimiter: "");
        size.IfPresent(s => newString = TextUtils.ApplySize(s, newString));
        return newString;
    }

    public ViewMode ViewMode() => viewMode;

    public INameModelComponent Clone()
    {
        List<PlayerControl> newViewerList = new(viewers());
        SimpleComponent component = new()
        {
            Prefixes = new List<Ubifix>(Prefixes),
            Suffixes = new List<Ubifix>(Suffixes),
            MainText = MainText,
            viewers = () => newViewerList,
            viewMode = viewMode,
            gameStates = gameStates,
            size = Optional<float>.From(size)
        };
        component.Prefixes.ForEach(p => p.AddComponent(component, 0));
        component.Suffixes.ForEach(p => p.AddComponent(component, 1));
        return component;
    }
}