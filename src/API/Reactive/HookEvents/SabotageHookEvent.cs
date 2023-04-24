using TOHTOR.API.Vanilla.Sabotages;

namespace TOHTOR.API.Reactive.HookEvents;

public class SabotageHookEvent: IHookEvent
{
    public ISabotage Sabotage;

    public SabotageHookEvent(ISabotage sabotage)
    {
        Sabotage = sabotage;
    }
}