namespace Lotus.Server.Interfaces;

public interface IRpcMarkHandler: IServerPatchHandler
{
    public void RpcMark(PlayerControl killer, PlayerControl? target, int colorId = 0);

    object? IServerPatchHandler.Execute(params object[] parameters)
    {
        RpcMark((PlayerControl)parameters[0], (PlayerControl?)parameters[1], (int)(parameters.Length > 2 ? parameters[2] : 0));
        return null;
    }
}