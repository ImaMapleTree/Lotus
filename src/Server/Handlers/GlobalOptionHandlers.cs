using Lotus.API;
using Lotus.Roles.Overrides;
using Lotus.Server.Interfaces;
using VentLib.Utilities.Collections;

namespace Lotus.Server.Handlers;

internal class GlobalOptionHandlers
{
    public static IGlobalOptionHandler StandardHandler = new Standard();
    public static IGlobalOptionHandler ProtectionPatchedHandler = new ProtectedPatch();

    private class Standard : IGlobalOptionHandler
    {
        public virtual RemoteList<GameOptionOverride> GetGlobalOptions()
        {
            RemoteList<GameOptionOverride> globalOverrides = new() { new GameOptionOverride(Override.ShapeshiftCooldown, 0.1f) };
            if (AUSettings.ConfirmImpostor()) globalOverrides.Add(new GameOptionOverride(Override.ConfirmEjects, false));
            return globalOverrides;
        }
    }

    private class ProtectedPatch: Standard
    {
        public override RemoteList<GameOptionOverride> GetGlobalOptions()
        {
            RemoteList<GameOptionOverride> globalOverrides =  base.GetGlobalOptions();
            globalOverrides.Add(new GameOptionOverride(Override.GuardianAngelDuration, 1000f));
            return globalOverrides;
        }
    }
}