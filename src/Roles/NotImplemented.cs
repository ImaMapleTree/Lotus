using Lotus.Options;
using VentLib.Logging;
using VentLib.Options.Game;

namespace Lotus.Roles;

public class NotImplemented: CustomRole
{
    protected override RoleModifier Modify(RoleModifier roleModifier)
    {
        VentLogger.Warn($"{this.RoleName} Not Implemented Yet", "RoleImplementation");
        return roleModifier;
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream).Tab(DefaultTabs.HiddenTab);
}