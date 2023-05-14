using Lotus.API.Odyssey;
using Lotus.GUI.Name.Impl;
using Lotus.API;

namespace Lotus.GUI.Name.Components;

public class IndicatorComponent : SimpleComponent
{
    public IndicatorComponent(LiveString mainText, GameState[] gameStates, ViewMode viewMode = Name.ViewMode.Additive, params PlayerControl[] viewers) : base(mainText, gameStates, viewMode, viewers)
    {
    }

    public IndicatorComponent(LiveString mainText, GameState gameState, ViewMode viewMode = Name.ViewMode.Additive, params PlayerControl[] viewers) : base(mainText, gameState, viewMode, viewers)
    {
    }

    public IndicatorComponent(string mainText, GameState gameState, ViewMode viewMode = Name.ViewMode.Additive, params PlayerControl[] viewers) : base(mainText, new []{gameState}, viewMode, viewers)
    {
    }

    public IndicatorComponent(string mainText, GameState[] gameStates, ViewMode viewMode = Name.ViewMode.Additive, params PlayerControl[] viewers) : base(mainText, gameStates, viewMode, viewers)
    {
    }
}