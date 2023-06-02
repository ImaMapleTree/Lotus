using UnityEngine;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.Vanilla;

public class Physicist : Scientist
{
    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) => 
        AddVitalsOptions(base.RegisterOptions(optionStream));

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.71f, 0.94f, 1f));
}