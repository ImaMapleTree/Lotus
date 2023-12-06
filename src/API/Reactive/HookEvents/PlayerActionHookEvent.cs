using Lotus.Roles.Internals;
using Lotus.Extensions;
using Lotus.Roles2.Operations;

namespace Lotus.API.Reactive.HookEvents;

public class PlayerActionHookEvent: IHookEvent
{
    public PlayerControl Source;
    public RoleAction Action;
    public object[] Params;

    public PlayerActionHookEvent(PlayerControl source, RoleAction action, object[] parameters)
    {
        Source = source;
        Action = action;
        Params = parameters;
    }

    public void Trigger()
    {
        ActionHandle handle = ActionHandle.NoInit();
        RoleOperations.Current.TriggerFor(new [] { Source }, Action.ActionType, Source, handle, Params);
    }
}