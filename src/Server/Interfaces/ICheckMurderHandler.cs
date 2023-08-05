namespace Lotus.Server.Interfaces;

public interface ICheckMurderHandler: IServerPatchHandler
{
    public void CheckMurder(PlayerControl killer, PlayerControl target);

    object? IServerPatchHandler.Execute(params object[] parameters)
    {
        CheckMurder((PlayerControl)parameters[0], (PlayerControl)parameters[1]);
        return null;
    }
}