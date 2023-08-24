namespace Lotus.API.Reactive.HookEvents;

public class EmptyHookEvent: IHookEvent
{
    public static readonly EmptyHookEvent Hook = new();
}