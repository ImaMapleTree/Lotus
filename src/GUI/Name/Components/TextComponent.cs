using System;
using System.Collections.Generic;
using TOHTOR.API;
using TOHTOR.GUI.Name.Impl;

namespace TOHTOR.GUI.Name.Components;

public class TextComponent : SimpleComponent
{
    public TextComponent(LiveString mainText, GameState[] gameStates, ViewMode viewMode = Impl.ViewMode.Additive, Func<List<PlayerControl>>? viewers = null) : base(mainText, gameStates, viewMode, viewers)
    {
    }

    public TextComponent(LiveString mainText, GameState[] gameStates, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : base(mainText, gameStates, viewMode, viewers)
    {
    }

    public TextComponent(LiveString mainText, GameState gameState, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : base(mainText, gameState, viewMode, viewers)
    {
    }

    public TextComponent(string mainText, GameState[] gameStates, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : base(mainText, gameStates, viewMode, viewers)
    {
    }
}