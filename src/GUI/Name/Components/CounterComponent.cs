using TOHTOR.API;
using TOHTOR.GUI.Counters;
using TOHTOR.GUI.Name.Impl;

namespace TOHTOR.GUI.Name.Components;

public class CounterComponent : NmComponent
{
    private readonly ICounter counter;

    public CounterComponent(ICounter counter, GameState[] gameStates, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : base("", gameStates, viewMode, viewers)
    {
        this.counter = counter;
        this.SetMainText(new LiveString(() => this.counter.CountString()));
    }
}