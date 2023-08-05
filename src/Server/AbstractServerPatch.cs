using System.Collections.Generic;
using System.Linq;
using Lotus.Server.Interfaces;
using VentLib.Utilities.Extensions;

namespace Lotus.Server;

public abstract class AbstractServerPatch: IModifyableServerPatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(AbstractServerPatch));

    protected virtual Dictionary<PatchedCode, IServerPatchHandler> CodePatchHandlers { get; set; } = new();

    public AbstractServerPatch()
    {
    }

    public AbstractServerPatch(AbstractServerPatch abstractServerPatch)
    {
        abstractServerPatch.CodePatchHandlers.ForEach(kv => CodePatchHandlers.TryAdd(kv.Key, kv.Value));
    }

    public virtual void Execute(PatchedCode patchedCodeType, params object?[] parameters) => ExecuteT<object>(patchedCodeType, parameters);

    public virtual T ExecuteT<T>(PatchedCode patchedCodeType, params object?[] parameters)
    {
        IServerPatchHandler? serverPatchHandler = FindHandler<IServerPatchHandler>(patchedCodeType);
        if (serverPatchHandler != null) return (T)serverPatchHandler.Execute(parameters)!;

        log.Warn($"Could not find {nameof(IServerPatchHandler)} for PatchedCode type \"{patchedCodeType}\"", "AbstractServerPatch::Execute");
        return default!;
    }

    public T? FindHandler<T>(PatchedCode patchedCodeType) where T: IServerPatchHandler => (T?)CodePatchHandlers.GetValueOrDefault(patchedCodeType);

    public T? FindHandlerByType<T>() where T : IServerPatchHandler => (T?)CodePatchHandlers.Values.FirstOrDefault(ph => ph.GetType() == typeof(T));
    public List<T> FindHandlersByType<T>() where T : IServerPatchHandler => CodePatchHandlers.Values.Where(ph => ph.GetType() == typeof(T)).Select(ph => (T)ph).ToList();

    public void SetHandler(PatchedCode patchedCode, IServerPatchHandler serverPatchHandler) => CodePatchHandlers[patchedCode] = serverPatchHandler;

    public IEnumerable<IServerPatchHandler> GetPatchHandlers() => CodePatchHandlers.Values;
}