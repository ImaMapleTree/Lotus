using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Lotus.Logging;
using Lotus.Roles;
using Lotus.Roles.Interfaces;
using Lotus.Server.Interfaces;
using VentLib.Utilities.Collections;

namespace Lotus.Server.Modifiers;

internal class PatchRoleInitializerModifier: IPatchModifier
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(PatchRoleInitializerModifier));

    public IServerPatch Modify(IServerPatch initialPatch)
    {
        DevLogger.Log("IServerPatch Modify Code");
        IRoleInitializerHandler? roleInitializerHandler = initialPatch.FindHandler<IRoleInitializerHandler>(PatchedCode.RoleInitializers);
        if (roleInitializerHandler?.RoleInitializers == null) return initialPatch;

        if (initialPatch is not IModifyableServerPatch modifyableServerPatch)
            throw new ConstraintException($"Cannot use {nameof(IRoleInitializerHandler)} on a non {nameof(IModifyableServerPatch)} server patch");

        List<IRoleInitializer> patchedRoleInitializers = new();
        PatchSyncedRoleInitializerHandler patchSyncHandler = new(patchedRoleInitializers);
        log.Debug("Finished creating synced role initializer handler");
        patchedRoleInitializers.AddRange(roleInitializerHandler.RoleInitializers.Select(initializer => new PatchSyncedRoleInitializer(patchSyncHandler, initializer)));

        modifyableServerPatch.SetHandler(PatchedCode.RoleInitializers, patchSyncHandler);
        return modifyableServerPatch;
    }

    public PatchModifierPriority Priority() => PatchModifierPriority.AbsoluteLast;

    private class PatchSyncedRoleInitializer : IRoleInitializer
    {
        private PatchSyncedRoleInitializerHandler parentRoleInitializerHandler;
        private IRoleInitializer initializer;

        public PatchSyncedRoleInitializer(PatchSyncedRoleInitializerHandler parentRoleInitializerHandler, IRoleInitializer initializer)
        {
            this.parentRoleInitializerHandler = parentRoleInitializerHandler;
            this.initializer = initializer;
            TargetType = initializer.TargetType;
        }

        public Type TargetType { get; }

        public void PreSetup(CustomRole role)
        {
            if (parentRoleInitializerHandler.IsEnabled) initializer.PreSetup(role);
        }

        public void PostModify(CustomRole role, AbstractBaseRole.RoleModifier roleModifier)
        {
            if (parentRoleInitializerHandler.IsEnabled) initializer.PostModify(role, roleModifier);
        }

        public CustomRole PostSetup(CustomRole role)
        {
            return parentRoleInitializerHandler.IsEnabled ? initializer.PostSetup(role) : role;
        }
    }

    private class PatchSyncedRoleInitializerHandler : IRoleInitializerHandler
    {
        public List<IRoleInitializer> RoleInitializers { get; }
        private List<IRemote>? initializerRemotes;
        public bool IsEnabled { get; private set; }

        public PatchSyncedRoleInitializerHandler(List<IRoleInitializer> roleInitializers)
        {
            RoleInitializers = roleInitializers;
        }

        public IServerPatchHandler Aggregate(IServerPatchHandler? lowerPatchHandler)
        {
            if (lowerPatchHandler is not IRoleInitializerHandler roleInitializerHandler) return this;
            return new PatchSyncedRoleInitializerHandler(this.RoleInitializers.Concat(roleInitializerHandler.RoleInitializers).ToList());
        }

        public void OnEnable(IServerPatch patch)
        {
            DevLogger.Log("ENABLIGN !Jksiaojoadsijosijdoisdajoiasdjioasdoi");
            initializerRemotes = RoleInitializers.Select(ri => (IRemote)ProjectLotus.RoleManager.AddRoleInitializers(ri.TargetType, ri)).ToList();
            IsEnabled = true;
        }

        public void OnDisable(IServerPatch patch)
        {
            initializerRemotes?.ForEach(remote => remote.Delete());
            IsEnabled = false;
        }
    }
}