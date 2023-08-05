namespace Lotus.Server.Interfaces;

public interface IRemoveProtectHandler: IServerPatchHandler
{
    public void RemoveProtection(PlayerControl player);

    object? IServerPatchHandler.Execute(params object[] parameters)
    {
        RemoveProtection((PlayerControl)parameters[0]);
        return null;
    }
}