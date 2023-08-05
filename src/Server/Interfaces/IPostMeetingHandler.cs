namespace Lotus.Server.Interfaces;

public interface IPostMeetingHandler: IServerPatchHandler
{
    public void PostMeetingSetup();

    object? IServerPatchHandler.Execute(params object?[] parameters)
    {
        PostMeetingSetup();
        return null;
    }
}