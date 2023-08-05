using AmongUs.GameOptions;
using VentLib.Options.Game;

namespace Lotus.Roles.Builtins.Vanilla;

public class ImpostorGhost : GuardianAngel
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(ImpostorGhost));

    protected override RoleModifier Modify(RoleModifier roleModifier)
    {
        base.Modify(roleModifier).VanillaRole(RoleTypes.CrewmateGhost);
        log.Warn($"{this.RoleName} Not Implemented Yet", "RoleImplementation");
        return roleModifier;
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream)
    {
        return new GameOptionBuilder();
    }
}