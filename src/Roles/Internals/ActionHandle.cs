using Lotus.Roles.Internals.Enums;

namespace Lotus.Roles.Internals;

public class ActionHandle
{
    public static ActionHandle NoInit() => new();

    public LotusActionType ActionType;
    public bool IsCanceled => Cancellation is not CancelType.None;
    public CancelType Cancellation;

    public ActionHandle(LotusActionType type)
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
        return $"ActionHandle(type={ActionType}, cancellation={Cancellation})";
    }

    public enum CancelType
    {
        None,
        Normal,
        Soft,
        Complete
    }
}