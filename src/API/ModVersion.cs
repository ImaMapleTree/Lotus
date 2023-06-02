using System.Linq;
using Lotus.API.Reactive;
using Lotus.Extensions;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;
using VentLib.Version;

namespace Lotus.API;

[LoadStatic]
public static class ModVersion
{
    public static VersionControl VersionControl = null!;
    public static Version Version => VersionControl.Version!;

    private static (bool isCached, bool allModded) _moddedStatus = (false, false);

    static ModVersion()
    {
        const string clientsModdedStatusHookKey = nameof(clientsModdedStatusHookKey);
        Hooks.PlayerHooks.PlayerJoinHook.Bind(clientsModdedStatusHookKey, _ => _moddedStatus.isCached = false);
    }

    public static bool AllClientsModded()
    {
        if (_moddedStatus.isCached) return _moddedStatus.allModded;
        _moddedStatus.isCached = true;
        return _moddedStatus.allModded = PlayerControl.AllPlayerControls.ToArray()
            .All(p =>
            {
                if (p == null || p.IsHost()) return true;
                return Version.Equals(VersionControl.GetPlayerVersion(p.PlayerId));
            });
    }
}