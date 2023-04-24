using TOHTOR.API;
using TOHTOR.GUI.Name.Impl;

namespace TOHTOR.GUI.Name.Components;

public class IndicatorComponent : SimpleComponent
{
    public IndicatorComponent(LiveString mainText, GameState[] gameStates, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : base(mainText, gameStates, viewMode, viewers)
    {
    }

    public IndicatorComponent(LiveString mainText, GameState gameState, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : base(mainText, gameState, viewMode, viewers)
    {
    }

    public IndicatorComponent(string mainText, GameState gameState, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : base(mainText, new []{gameState}, viewMode, viewers)
    {
    }

    public IndicatorComponent(string mainText, GameState[] gameStates, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : base(mainText, gameStates, viewMode, viewers)
    {
    }
}