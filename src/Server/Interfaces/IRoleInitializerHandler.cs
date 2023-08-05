using System.Collections.Generic;
using Lotus.Roles.Interfaces;

namespace Lotus.Server.Interfaces;

public interface IRoleInitializerHandler: IServerPatchHandler
{
    public List<IRoleInitializer> RoleInitializers { get; }

    object? IServerPatchHandler.Execute(params object?[] parameters) => null;
}