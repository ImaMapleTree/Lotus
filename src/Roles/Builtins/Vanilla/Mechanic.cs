using UnityEngine;
using VentLib.Options.Game;

namespace Lotus.Roles.Builtins.Vanilla;

public class Mechanic : Engineer
{
    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddVentingOptions(base.RegisterOptions(optionStream));

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(new Color(1f, 0.65f, 0.04f));
}