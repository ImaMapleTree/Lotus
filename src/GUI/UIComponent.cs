using System;
using TOHTOR.API;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Impl;

namespace TOHTOR.GUI;

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