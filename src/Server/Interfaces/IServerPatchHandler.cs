namespace Lotus.Server.Interfaces;

public interface IServerPatchHandler
{
    public IServerPatchHandler Aggregate(IServerPatchHandler? lowerPatchHandler) => this;

    public object? Execute(params object?[] parameters);

    public void OnEnable(IServerPatch patch) {}

    public void OnDisable(IServerPatch patch) {}
}