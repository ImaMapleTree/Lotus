using Lotus.API.Vanilla.Sabotages;

namespace Lotus.API.Reactive.HookEvents;

public class SabotageHookEvent: IHookEvent
{
    public ISabotage Sabotage;

    public SabotageHookEvent(ISabotage sabotage)
    {
        Sabotage = sabotage;
    }
}