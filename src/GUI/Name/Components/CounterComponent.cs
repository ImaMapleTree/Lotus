using System;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.GUI.Counters;
using TOHTOR.GUI.Name.Impl;

namespace TOHTOR.GUI.Name.Components;

public class CounterComponent : SimpleComponent
{
    private readonly ICounter? counter;
    private Func<ICounter>? counterSupplier;

    public CounterComponent(ICounter counter, GameState[] gameStates, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : base("", gameStates, viewMode, viewers)
    {
        this.counter = counter;
        this.SetMainText(new LiveString(() => this.counter.CountString()));
    }

    public CounterComponent(Func<ICounter> counterSupplier, GameState[] gameStates, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : base("", gameStates, viewMode, viewers)
    {
        this.counterSupplier = counterSupplier ;
        this.SetMainText(new LiveString(() => Counter().CountString()));
    }

    public CounterComponent(LiveString mainText, GameState[] gameStates, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : base(mainText, gameStates, viewMode, viewers)
    {
    }

    public ICounter Counter() => counter ?? counterSupplier!();
}