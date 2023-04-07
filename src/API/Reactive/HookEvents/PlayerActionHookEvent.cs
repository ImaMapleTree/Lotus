using TOHTOR.Extensions;
using TOHTOR.Roles.Internals;

namespace TOHTOR.API.Reactive.HookEvents;

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
        Source.Trigger(Action.ActionType, ref handle, Params);
    }
}