extern alias JBAnnotations;
using System;
using JBAnnotations::JetBrains.Annotations;
using Lotus.API.Odyssey;
using Lotus.GUI.Name;

namespace Lotus.GUI;

[UsedImplicitly]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
public class UIComponent: Attribute
{
    public UI Component;
    public ViewMode ViewMode;
    public GameState[] GameStates;

    public UIComponent(UI component, ViewMode viewMode = ViewMode.Additive, params GameState[] gameStates)
    {
        this.Component = component;
        this.ViewMode = viewMode;
        this.GameStates = gameStates.Length > 0 ? gameStates : new [] { GameState.Roaming };
    }
}