using UnityEngine;

namespace Lotus.Roles.RoleGroups.Vanilla;

public class Physicist : Scientist
{
    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.71f, 0.94f, 1f));
}