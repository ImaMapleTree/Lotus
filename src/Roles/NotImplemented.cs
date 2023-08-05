using Lotus.Options;
using VentLib.Options.Game;

namespace Lotus.Roles;

public class NotImplemented: CustomRole
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(NotImplemented));

    protected override RoleModifier Modify(RoleModifier roleModifier)
    {
        log.Warn($"{this.RoleName} Not Implemented Yet", "RoleImplementation");
        return roleModifier;
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream).Tab(DefaultTabs.HiddenTab);
}