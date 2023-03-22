using TOHTOR.API;
using UnityEngine;
using VentLib.Utilities;

namespace TOHTOR.GUI.Name.Components;

public class SimpleIndicatorComponent : IndicatorComponent
{
    public SimpleIndicatorComponent(string mainText, Color color, GameState gameState, params PlayerControl[] viewers) : base(color.Colorize(mainText), gameState, Impl.ViewMode.Additive, viewers)
    {
    }

    public SimpleIndicatorComponent(string mainText, Color color, GameState[] gameStates, params PlayerControl[] viewers) : base(color.Colorize(mainText), gameStates, Impl.ViewMode.Additive, viewers)
    {
    }
}