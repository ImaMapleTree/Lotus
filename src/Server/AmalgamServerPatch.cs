using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.Server.Interfaces;
using VentLib.Utilities.Extensions;

namespace Lotus.Server;

public class AmalgamServerPatch: AbstractServerPatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(AmalgamServerPatch));

    private readonly Dictionary<PatchedCode, (IServerPatch?, Type?)> patchOriginInfo = new();

    public AmalgamServerPatch(List<IServerPatch> serverPatches)
    {
        Enum.GetValues<PatchedCode>().ForEach(pc =>
        {
            IServerPatchHandler? usedPatchHandler = serverPatches
                .Where(patch => patch.FindHandler<IServerPatchHandler>(pc) != null)
                .Select(patch => patch.FindHandler<IServerPatchHandler>(pc))
                .Aggregate((IServerPatchHandler)null!, (aggregate, patchHandler) => patchHandler?.Aggregate(aggregate)!);

            if (usedPatchHandler == null!) return;

            IServerPatch? usedPatch = serverPatches.FirstOrDefault(sp => sp.GetPatchHandlers().Contains(usedPatchHandler));

            CodePatchHandlers[pc] = usedPatchHandler;
            patchOriginInfo[pc] = (usedPatch, usedPatch?.GetType());
        });
    }

    public override T ExecuteT<T>(PatchedCode patchedCodeType, params object?[] parameters)
    {
        log.Trace($"Executing patch {patchedCodeType} from \"{patchOriginInfo.GetOptional(patchedCodeType).Map(pc => pc.Item2?.Name).OrElse("Unknown")}\"", "Execute");
        return base.ExecuteT<T>(patchedCodeType, parameters);
    }

    public IServerPatch? GetUsedPatch(PatchedCode patchedCodeType) => patchOriginInfo[patchedCodeType].Item1;
}