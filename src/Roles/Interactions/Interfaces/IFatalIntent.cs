namespace Lotus.Roles.Interactions.Interfaces;

public interface IFatalIntent : IKillingIntent
{
    public bool IsRanged();
}