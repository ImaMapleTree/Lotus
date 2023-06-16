using System;
using System.Collections.Generic;
using Lotus.API.Odyssey;
using Lotus.GUI.Name.Impl;
using Lotus.API;

namespace Lotus.GUI.Name.Components;

public class NameComponent : SimpleComponent
{
    public NameComponent(LiveString mainText, GameState[] gameStates, ViewMode viewMode = Name.ViewMode.Additive, params PlayerControl[] viewers) : base(mainText, gameStates, viewMode, viewers)
    {
    }

    public NameComponent(LiveString mainText, GameState[] gameStates, ViewMode viewMode = Name.ViewMode.Additive, Func<List<PlayerControl>>? viewers = null) : base(mainText, gameStates, viewMode, viewers)
    {
    }

    public NameComponent(LiveString mainText, GameState gameState, ViewMode viewMode = Name.ViewMode.Additive, params PlayerControl[] viewers) : base(mainText, gameState, viewMode, viewers)
    {
    }

    public NameComponent(string mainText, GameState[] gameStates, ViewMode viewMode = Name.ViewMode.Additive, params PlayerControl[] viewers) : base(mainText, gameStates, viewMode, viewers)
    {
    }

    public NameComponent(PlayerControl player, GameState[] gameStates, ViewMode viewMode = Name.ViewMode.Additive, Func<List<PlayerControl>>? viewers = null) : base(new LiveString(player.name), gameStates, viewMode, viewers)
    {
    }
}