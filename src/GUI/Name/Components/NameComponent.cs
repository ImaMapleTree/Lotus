using System;
using System.Collections.Generic;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.GUI.Name.Impl;

namespace TOHTOR.GUI.Name.Components;

public class NameComponent : SimpleComponent
{
    public NameComponent(LiveString mainText, GameState[] gameStates, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : base(mainText, gameStates, viewMode, viewers)
    {
    }

    public NameComponent(LiveString mainText, GameState[] gameStates, ViewMode viewMode = Impl.ViewMode.Additive, Func<List<PlayerControl>>? viewers = null) : base(mainText, gameStates, viewMode, viewers)
    {
    }

    public NameComponent(LiveString mainText, GameState gameState, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : base(mainText, gameState, viewMode, viewers)
    {
    }

    public NameComponent(string mainText, GameState[] gameStates, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : base(mainText, gameStates, viewMode, viewers)
    {
    }

    public NameComponent(PlayerControl player, GameState[] gameStates, ViewMode viewMode = Impl.ViewMode.Additive, Func<List<PlayerControl>>? viewers = null) : base(new LiveString(player.NameModel().Unaltered), gameStates, viewMode, viewers)
    {
    }
}