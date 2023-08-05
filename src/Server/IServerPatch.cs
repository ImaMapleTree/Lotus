using System.Collections.Generic;
using Lotus.Server.Interfaces;

namespace Lotus.Server;

public interface IServerPatch
{
    public void Execute(PatchedCode patchedCodeType, params object?[] parameters) => ExecuteT<object>(patchedCodeType, parameters);

    public T ExecuteT<T>(PatchedCode patchedCodeType, params object?[] parameters);

    public T? FindHandler<T>(PatchedCode patchedCodeType) where T: IServerPatchHandler;

    public T? FindHandlerByType<T>() where T : IServerPatchHandler;

    public List<T> FindHandlersByType<T>() where T : IServerPatchHandler;

    public IEnumerable<IServerPatchHandler> GetPatchHandlers();
}