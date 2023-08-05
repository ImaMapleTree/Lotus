namespace Lotus.Server.Interfaces;

public interface IMurderPlayerHandler: IServerPatchHandler
{
    public void MurderPlayer(PlayerControl killer, PlayerControl target);

    object? IServerPatchHandler.Execute(params object[] parameters)
    {
        MurderPlayer((PlayerControl)parameters[0], (PlayerControl)parameters[1]);
        return null;
    }
}