using Lotus.Roles.Internals.Attributes;

namespace Lotus.Roles.Internals;

public class ActionHandle
{
    public static ActionHandle NoInit() => new();

    public RoleActionType ActionType;
    public bool IsCanceled => Cancellation is not CancelType.None;
    public CancelType Cancellation;

    public ActionHandle(RoleActionType type)
    {
        this.ActionType = type;
    }

    private ActionHandle() { }

    public void Cancel(CancelType cancelType = CancelType.Normal)
    {
        this.Cancellation = cancelType;
    }

    public override string ToString()
    {
        return $"ActionHandle(type={ActionType}, cancelled={IsCanceled})";
    }

    public enum CancelType
    {
        None,
        Normal,
        Complete
    }
}