using Lotus.API.Odyssey;
using UnityEngine;
using VentLib.Utilities;

namespace Lotus.GUI.Name.Components;

public class SimpleIndicatorComponent : IndicatorComponent
{
    public SimpleIndicatorComponent(string mainText, Color color, GameState gameState, params PlayerControl[] viewers) : base(color.Colorize(mainText), gameState, Name.ViewMode.Additive, viewers)
    {
    }

    public SimpleIndicatorComponent(string mainText, Color color, GameState[] gameStates, params PlayerControl[] viewers) : base(color.Colorize(mainText), gameStates, Name.ViewMode.Additive, viewers)
    {
    }
}